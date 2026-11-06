using System.Windows;
using System.Windows.Controls;
using EnglishB2Academy.Core;

namespace EnglishB2Academy;

public partial class MainWindow
{
    private void ShowMistakes()
    {
        AddHeading("A mistake is a useful next step", "YOUR MISTAKE NOTEBOOK");
        var sessions = store.Sessions().Where(s => s.Finished is not null).ToArray();
        var latest = sessions.SelectMany(s => s.Questions.Select(q =>
            (Question: q, Response: s.Responses.GetValueOrDefault(q.Id, ""), Date: s.Finished!.Value)))
            .GroupBy(x => x.Question.Id).Select(g => g.OrderByDescending(x => x.Date).First()).ToArray();
        var mistakes = latest.Where(x => !Grading.IsCorrect(x.Question, x.Response)).ToArray();
        if (mistakes.Length == 0)
        {
            SecondaryPage.Children.Add(Card(Text("No unresolved mistakes recorded yet.", 20),
                Text("Complete a lesson check or practice test to build a useful revision record.", 15, muted: true),
                Action("Open courses", () => Navigate("English courses"))));
            return;
        }
        SecondaryPage.Children.Add(Card(Text($"{mistakes.Length} questions to revisit", 22),
            Text("Only the most recent response to each question is used here. Correct answers in a later revision clear it from this list, while the full history remains.", 14, muted: true),
            Action("Create revision test · up to 20 questions", () =>
                StartSession(PracticeFactory.Revision(mistakes.Select(m => m.Question), DateTimeOffset.UtcNow)))));
        foreach (var item in mistakes)
            SecondaryPage.Children.Add(Card(Text($"{item.Question.Level} · {item.Question.Skill}", 12, muted: true),
                Text(item.Question.Prompt, 18), Text($"Your last answer: {(item.Response.Length == 0 ? "(unanswered)" : item.Response)}", 15),
                Text($"Accepted: {string.Join(" / ", item.Question.Answers)}", 15),
                Text(item.Question.Explanation, 15, muted: true)));
    }

    private void ShowProgress()
    {
        AddHeading("Look at what you have practised", "YOUR PROGRESS");
        var sessions = store.Sessions().Where(s => s.Finished is not null).ToArray();
        var passed = Curriculum.Lessons.Count(l => sessions.Any(s => s.SourceId == l.Id && s.Mode == "Lesson" && s.Accuracy >= 80));
        var minutes = sessions.Sum(s => s.ActiveSeconds) / 60;
        SecondaryPage.Children.Add(Card(Text($"{passed}/{Curriculum.Lessons.Count} lesson checks passed · {sessions.Length} activities", 24),
            Text($"{Grading.Streak(sessions, DateTime.Today)}-day activity streak · {minutes} active practice minutes (objective sessions only)", 16),
            Text("Passing a short lesson check is not the same as mastering a skill. Writing, speaking, reading lesson text and time away from the activity window are not included in study minutes.", 14, muted: true)));
        AddHeading("Seven-day activity", "COMPLETED OBJECTIVE ACTIVITIES");
        for (var offset = -6; offset <= 0; offset++)
        {
            var day = DateTime.Today.AddDays(offset);
            var count = sessions.Count(s => s.Finished!.Value.LocalDateTime.Date == day);
            SecondaryPage.Children.Add(Bar($"{day:ddd dd} · {count} completed", count, Math.Max(1, sessions.GroupBy(s => s.Finished!.Value.LocalDateTime.Date).Select(g => g.Count()).DefaultIfEmpty(1).Max())));
        }
        AddHeading("Accuracy by skill", "RAW PRACTICE, INCLUDING REPEATED QUESTIONS");
        foreach (var skill in new[] { "Grammar", "Vocabulary", "Reading", "Listening" })
        {
            var responses = sessions.SelectMany(s => s.Questions.Where(q => q.Skill == skill)
                .Select(q => Grading.IsCorrect(q, s.Responses.GetValueOrDefault(q.Id, "")))).ToArray();
            var percent = responses.Length == 0 ? 0 : 100d * responses.Count(x => x) / responses.Length;
            SecondaryPage.Children.Add(Bar($"{skill}: {(responses.Length == 0 ? "No evidence" : $"{percent:0}% across {responses.Length} responses")}", percent, 100));
        }
        SecondaryPage.Children.Add(Card(Text("Writing and speaking", 20),
            Text($"{store.Drafts().Count} saved drafts. Writing and speaking quality have not been independently assessed; they are excluded from automatic accuracy charts.", 15, muted: true)));
        AddHeading("Activity history", "TRANSPARENT LOCAL RECORD");
        foreach (var session in sessions)
            SecondaryPage.Children.Add(Card(Text(session.Title, 20),
                Text($"{session.Finished!.Value.LocalDateTime:g} · {session.Mode} · {session.Correct}/{session.Questions.Count} ({session.Accuracy:0}%)", 14, muted: true),
                Action("Review saved answers", () => ShowResults(session))));
    }

    private Border Bar(string label, double value, double maximum)
    {
        var bar = new ProgressBar { Minimum = 0, Maximum = maximum, Value = value, Height = 10, Margin = new Thickness(0, 0, 0, 8) };
        System.Windows.Automation.AutomationProperties.SetName(bar, label);
        return Card(Text(label, 15), bar);
    }

    private void ShowReadiness()
    {
        AddHeading("Am I ready for my B2 exam?", "EVIDENCE, NOT A PROMISE");
        var exams = store.Sessions().Where(s => s.Mode == "Exam" && s.Finished is not null).ToArray();
        var diagnostic = store.Sessions().FirstOrDefault(s => s.Mode == "Diagnostic" && s.Finished is not null);
        var profile = store.Profile();
        var countdown = DateTime.TryParse(profile.ExamDate, out var date) ? $"{(date.Date - DateTime.Today).Days} days to your target date." : "No target examination date set.";
        SecondaryPage.Children.Add(Notice("Readiness not established",
            "This release cannot responsibly classify you as likely ready. It lacks unseen full-length mocks and independently assessed writing/speaking. No practice percentage is converted to the Cambridge English Scale."));
        SecondaryPage.Children.Add(Card(Text("Available evidence", 22),
            Text($"{exams.Length} completed short timed practices. {(diagnostic is null ? "No diagnostic result." : Grading.DiagnosticLabel(diagnostic) + ".")}", 16),
            Text(countdown, 15, muted: true),
            Text("Still needed: representative unseen tasks, reliable writing/speaking assessment, full-paper timing, repeated recent results and performance consistency. Completing this library alone is insufficient.", 16),
            Action("Review practice history", () => Navigate("My progress")),
            Action("Focus on recorded mistakes", () => Navigate("My mistakes")),
            Action("Set target date", ShowProfile)));
        if (exams.Length > 0)
            SecondaryPage.Children.Add(Card(Text("Recent timed practice (not official marks)", 20),
                Text(string.Join("\n", exams.Take(5).Select(s => $"{s.Finished!.Value.LocalDateTime:d}: {s.Title} — {s.Correct}/{s.Questions.Count}")), 15)));
    }
}
