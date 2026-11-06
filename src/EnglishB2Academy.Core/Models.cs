namespace EnglishB2Academy.Core;

public sealed record Question(
    string Id, string Skill, string Level, string Prompt, string[] Options,
    string[] Answers, string Explanation, string Passage = "", string Audio = "");

public sealed record Lesson(
    string Id, string Level, string Skill, string Title, string Explanation,
    string Example, string CommonMistake, Question[] Questions);

public sealed record ExamProfile(string Id, string Name, string Source, string Verified,
    ExamPaper[] Papers);

public sealed record ExamPaper(string Name, int Minutes, int Parts, int Questions, string Instructions);

public sealed record LearnerProfile(string KnownLevel = "Not sure", string FirstLanguage = "",
    string Variety = "British English", string ExamDate = "", string Pace = "Balanced");

public sealed record PracticeSession
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = "";
    public string SourceId { get; init; } = "";
    public string Mode { get; init; } = "Practice";
    public DateTimeOffset Started { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Deadline { get; init; }
    public DateTimeOffset? Finished { get; init; }
    public List<Question> Questions { get; init; } = [];
    public Dictionary<string, string> Responses { get; init; } = [];
    public Dictionary<string, int> AudioPlays { get; init; } = [];
    public int Position { get; set; }
    public long ActiveSeconds { get; set; }

    public bool IsExpired(DateTimeOffset now) => Deadline is { } deadline && now >= deadline;
    public int Correct => Questions.Count(q => Grading.IsCorrect(q, Responses.GetValueOrDefault(q.Id, "")));
    public double Accuracy => Questions.Count == 0 ? 0 : 100d * Correct / Questions.Count;
}

public sealed record Draft(string Id, string Kind, string Prompt, string Text, DateTimeOffset Updated);
public sealed record VocabularyItem(string Word, string Meaning, string Example);
public sealed record ReviewCard(string Word, int IntervalDays, DateTimeOffset Due);
