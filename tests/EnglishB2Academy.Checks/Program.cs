using System.Text.Json;
using EnglishB2Academy;
using EnglishB2Academy.Core;
using Microsoft.Data.Sqlite;

var directory = Path.Combine(Path.GetTempPath(), "EnglishB2Academy-checks-" + Guid.NewGuid().ToString("N"));
var path = Path.Combine(directory, "preferences.json");
var store = new PreferencesStore(path);
var passed = 0;

void Check(string name, Action test)
{
    test();
    passed++;
    Console.WriteLine($"PASS {name}");
}

void Assert(bool condition)
{
    if (!condition) throw new InvalidOperationException("Assertion failed.");
}

void Reject<T>(string json) where T : Exception
{
    File.WriteAllText(path, json);
    try { store.Load(); }
    catch (T) { Assert(File.ReadAllText(path) == json); return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}

try
{
    Check("First launch uses honest defaults without creating a file", () =>
    {
        Assert(store.Load() == new Preferences());
        Assert(!File.Exists(path));
    });
    Check("Preferences round-trip and create their directory", () =>
    {
        var saved = new Preferences { Theme = "Dark", DailyMinutes = 30, TextSize = "Large" };
        store.Save(saved);
        Assert(store.Load() == saved);
    });
    Check("Replacing preferences is persistent and leaves no temporary file", () =>
    {
        store.Save(new Preferences());
        Assert(new PreferencesStore(path).Load() == new Preferences());
        Assert(!File.Exists(path + ".tmp"));
    });
    Check("Malformed JSON is reported and preserved", () => Reject<JsonException>("{not json"));
    Check("Unsupported theme is reported and preserved", () => Reject<InvalidDataException>("""{"Theme":"Unknown"}"""));
    Check("Unsupported goal is reported and preserved", () => Reject<InvalidDataException>("""{"DailyMinutes":-1}"""));
    Check("Unsupported text size is reported and preserved", () => Reject<InvalidDataException>("""{"TextSize":"Tiny"}"""));
    Check("Null document is reported and preserved", () => Reject<InvalidDataException>("null"));
    var now = new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
    var lessons = Curriculum.Lessons;
    Check("All 35 original lessons have unique IDs and consistent answer keys", () =>
    {
        Curriculum.Validate(lessons);
        Assert(lessons.Count == 35 && lessons.Sum(l => l.Questions.Length) == 105);
        foreach (var level in Curriculum.Levels)
            Assert(lessons.Count(l => l.Level == level) == 7);
        foreach (var question in lessons.SelectMany(l => l.Questions))
        {
            Assert(question.Options.Length == 3);
            Assert(question.Answers.Length == 1);
            Assert(question.Options.Count(option => Grading.IsCorrect(question, option)) == 1);
        }
    });
    Check("Original exam bank has exactly 22 language/reading and 8 listening items", () =>
    {
        var exam = PracticeFactory.Exam("Mock", now);
        Assert(exam.Questions.Count == 30);
        Assert(exam.Questions.Count(q => q.Skill == "Listening") == 8);
        Assert(exam.Questions.Select(q => q.Id).Distinct().Count() == 30);
        Assert(!exam.Questions.Any(q => lessons.SelectMany(l => l.Questions).Any(lq => lq.Id == q.Id)));
        Assert(exam.Deadline == now.AddMinutes(45));
        foreach (var q in exam.Questions)
        {
            Assert(q.Answers.Length > 0 && q.Explanation.Length > 0);
            Assert(q.Options.Length == 0 || q.Answers.All(a => q.Options.Contains(a)));
        }
    });
    Check("Key-word transformations use 2-5 words and retain the key word", () =>
    {
        foreach (var q in PracticeFactory.ExamQuestions().Where(q => q.Id.StartsWith("exam-transform-")))
        {
            var keyword = q.Prompt.Split(", ")[1].Split(' ')[0];
            foreach (var answer in q.Answers)
            {
                Assert(Grading.WordCount(answer) is >= 2 and <= 5);
                Assert(answer.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }
        }
    });
    Check("Answer normalisation accepts whitespace/case but not incorrect grammar", () =>
    {
        var q = lessons.SelectMany(l => l.Questions).First();
        Assert(Grading.IsCorrect(q, "  IS  "));
        Assert(!Grading.IsCorrect(q, "isn't"));
        Assert(!Grading.IsCorrect(q, ""));
        Assert(Grading.Normalise("  DON'T  \n  KNOW ") == "don't know");
    });
    Check("Unanswered questions are incorrect and denominator remains fixed", () =>
    {
        var session = PracticeFactory.Lesson(lessons[0]);
        session.Responses[session.Questions[0].Id] = session.Questions[0].Answers[0];
        Assert(session.Correct == 1 && Math.Abs(session.Accuracy - 100d / 3) < .001);
    });
    Check("Timer expires exactly at the deadline, not a second earlier", () =>
    {
        var exam = PracticeFactory.Exam("Mock", now);
        Assert(!exam.IsExpired(now.AddMinutes(45).AddSeconds(-1)));
        Assert(exam.IsExpired(now.AddMinutes(45)));
        Assert(exam.IsExpired(now.AddDays(1)));
        Assert(!PracticeFactory.Lesson(lessons[0]).IsExpired(now.AddYears(1)));
    });
    Check("Diagnostic samples three questions per level and is conservative", () =>
    {
        var diagnostic = PracticeFactory.Diagnostic(now);
        Assert(diagnostic.Questions.Count == 15);
        Assert(Curriculum.Levels.All(level => diagnostic.Questions.Count(q => q.Level == level) == 3));
        Assert(Grading.DiagnosticLabel(diagnostic) == "Not assessed");
        foreach (var q in diagnostic.Questions) diagnostic.Responses[q.Id] = q.Answers[0];
        diagnostic = diagnostic with { Finished = now.AddMinutes(1) };
        Assert(Grading.DiagnosticLabel(diagnostic) == "B2 practice starting point");
        diagnostic.Responses[diagnostic.Questions.First(q => q.Level == "A1").Id] = "";
        Assert(Grading.DiagnosticLabel(diagnostic) == "Start with A1");
    });
    Check("Word count handles apostrophes, hyphens and repeated spacing", () =>
    {
        Assert(Grading.WordCount("It's a well-known place.") == 4);
        Assert(Grading.WordCount(" \n ") == 0);
        Assert(Grading.WritingFeedback(new string('a', 1)).Contains("139"));
        Assert(Grading.WritingFeedback(string.Join(" ", Enumerable.Repeat("word", 140))).Contains("within"));
        Assert(Grading.WritingFeedback(string.Join(" ", Enumerable.Repeat("word", 190))).Contains("within"));
        Assert(Grading.WritingFeedback(string.Join(" ", Enumerable.Repeat("word", 191))).Contains("removing about 1"));
    });
    var database = Path.Combine(directory, "learning.db");
    using (var learning = new LearningStore(database))
    {
        Check("SQLite creates schema and empty defaults", () =>
        {
            Assert(learning.Sessions().Count == 0 && learning.Drafts().Count == 0);
            Assert(learning.Profile() == new LearnerProfile());
        });
        var attempt = PracticeFactory.Exam("Mock", now);
        attempt.Responses[attempt.Questions[0].Id] = attempt.Questions[0].Answers[0];
        attempt.AudioPlays["clip"] = 2;
        attempt.Position = 5;
        attempt.ActiveSeconds = 12;
        Check("SQLite persists answers, position, audio plays and absolute deadline", () =>
        {
            learning.SaveSession(attempt);
            var restored = learning.Sessions().Single();
            Assert(restored.Correct == 1 && restored.Position == 5 && restored.AudioPlays["clip"] == 2);
            Assert(restored.Deadline == now.AddMinutes(45) && restored.ActiveSeconds == 12);
        });
        Check("A completed attempt updates one row without losing review answers", () =>
        {
            learning.SaveSession(attempt with { Finished = now.AddMinutes(10) });
            Assert(learning.Sessions().Count == 1 && learning.Sessions()[0].Finished is not null);
            Assert(learning.Sessions()[0].Responses.Count == 1);
        });
        Check("Drafts and profiles preserve text safely with quotes and SQL characters", () =>
        {
            learning.SaveDraft(new Draft("draft-1", "Writing", "Original prompt", "It's safe; DROP TABLE sessions; --", now));
            learning.SaveProfile(new LearnerProfile("B1", "Optional language", "British English", "2027-01-01", "Balanced"));
            Assert(learning.Drafts()[0].Text.Contains("DROP TABLE"));
            Assert(learning.Profile().KnownLevel == "B1" && learning.Sessions().Count == 1);
        });
        Check("Spaced repetition schedules 1, 2, 4 days and resets to ten minutes", () =>
        {
            learning.Review("evidence", true, now);
            Assert(learning.ReviewCards().Single().Due == now.AddDays(1));
            learning.Review("evidence", true, now);
            Assert(learning.ReviewCards().Single().IntervalDays == 2);
            learning.Review("evidence", true, now);
            Assert(learning.ReviewCards().Single().IntervalDays == 4);
            learning.Review("evidence", false, now);
            Assert(learning.ReviewCards().Single().Due == now.AddMinutes(10));
        });
        var backup = Path.Combine(directory, "backup.db");
        Check("Live SQLite backup validates, restores and preserves all record types", () =>
        {
            learning.Backup(backup);
            LearningStore.ValidateBackup(backup);
            learning.DeleteLearningRecords();
            Assert(learning.Sessions().Count == 0 && learning.Drafts().Count == 0);
            learning.Restore(backup);
            Assert(learning.Sessions().Count == 1 && learning.Drafts().Count == 1 &&
                learning.ReviewCards().Count == 1 && learning.Profile().KnownLevel == "B1");
        });
        Check("Backup refuses to overwrite an existing backup or the live database", () =>
        {
            try { learning.Backup(backup); throw new Exception("Expected existing-file failure."); }
            catch (IOException) { }
            try { learning.Backup(database); throw new Exception("Expected same-file failure."); }
            catch (InvalidDataException) { }
        });
        Check("Corrupt backup is rejected without changing live records", () =>
        {
            var corrupt = Path.Combine(directory, "corrupt.db");
            File.WriteAllText(corrupt, "not a database");
            try { learning.Restore(corrupt); throw new Exception("Expected invalid backup failure."); }
            catch (SqliteException) { }
            Assert(learning.Sessions().Count == 1);
        });
        Check("Valid SQLite with invalid JSON session is rejected before restore", () =>
        {
            var invalid = Path.Combine(directory, "invalid.db");
            File.Copy(backup, invalid);
            using (var sql = new SqliteConnection($"Data Source={invalid};Pooling=False"))
            {
                sql.Open();
                using var command = sql.CreateCommand();
                command.CommandText = """UPDATE sessions SET payload='{"Questions":null}'""";
                command.ExecuteNonQuery();
            }
            try { learning.Restore(invalid); throw new Exception("Expected invalid session rejection."); }
            catch (InvalidDataException) { }
            Assert(learning.Sessions().Single().Correct == 1);
        });
        Check("Invalid sessions cannot be saved", () =>
        {
            try { learning.SaveSession(attempt with { Position = 999 }); throw new Exception("Expected invalid position failure."); }
            catch (InvalidDataException) { }
            Assert(learning.Sessions().Single().Position == 5);
        });
        Check("JSON export contains typed records and schema without audio files", () =>
        {
            using var json = JsonDocument.Parse(learning.ExportJson());
            Assert(json.RootElement.GetProperty("Schema").GetInt32() == 1);
            Assert(json.RootElement.GetProperty("Sessions").GetArrayLength() == 1);
            Assert(json.RootElement.GetProperty("Drafts").GetArrayLength() == 1);
        });
    }
    Check("Database reopens persistently after disposal", () =>
    {
        using var reopened = new LearningStore(database);
        Assert(reopened.Sessions().Count == 1 && reopened.Drafts().Count == 1);
    });
    Check("Recommendation advances only after a passed lesson check", () =>
    {
        var lesson = lessons[0];
        var complete = PracticeFactory.Lesson(lesson) with { Finished = DateTimeOffset.UtcNow };
        foreach (var q in complete.Questions) complete.Responses[q.Id] = q.Answers[0];
        Assert(Grading.Recommend(lessons, [complete]).Id != lesson.Id);
        Assert(Grading.Recommend(lessons, []).Id == lesson.Id);
    });
    Check("Streak counts distinct local dates and permits yesterday as latest activity", () =>
    {
        var today = DateTime.Today;
        var one = PracticeFactory.Lesson(lessons[0]) with { Finished = new DateTimeOffset(today.AddHours(12)) };
        var two = one with { Finished = new DateTimeOffset(today.AddDays(-1).AddHours(12)) };
        Assert(Grading.Streak([one, one, two], today) == 2);
        Assert(Grading.Streak([two], today) == 1);
        Assert(Grading.Streak([], today) == 0);
    });
    Console.WriteLine($"{passed} checks passed.");
}
finally
{
    if (Directory.Exists(directory))
        foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
    if (Directory.Exists(directory)) Directory.Delete(directory);
}
