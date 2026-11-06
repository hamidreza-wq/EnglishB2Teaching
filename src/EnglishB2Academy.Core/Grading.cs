using System.Text;
using System.Text.RegularExpressions;

namespace EnglishB2Academy.Core;

public static partial class Grading
{
    public static string Normalise(string text) => Whitespace().Replace(
        text.Normalize(NormalizationForm.FormKC).Replace('\u2019', '\'').Trim().ToLowerInvariant(), " ");

    public static bool IsCorrect(Question question, string response) =>
        !string.IsNullOrWhiteSpace(response) &&
        question.Answers.Any(answer => Normalise(answer) == Normalise(response));

    public static int WordCount(string text) => Words().Matches(text).Count;

    public static string WritingFeedback(string text)
    {
        var count = WordCount(text);
        var length = count < 140 ? $"Add about {140 - count} words to reach 140." :
            count > 190 ? $"Consider removing about {count - 190} words to stay within 190." :
            "Your draft is within the 140-190 word practice range.";
        var paragraphs = ParagraphBreak().Split(text.Trim()).Count(p => !string.IsNullOrWhiteSpace(p));
        return $"{count} words. {length}\n{paragraphs} paragraph(s). " +
            "Review your purpose, paragraph organisation, supporting examples, register and sentence accuracy. " +
            "This is a mechanical length/structure check, not an assessment of English quality or an exam mark.";
    }

    public static string DiagnosticLabel(PracticeSession session)
    {
        if (session.Mode != "Diagnostic" || session.Finished is null) return "Not assessed";
        var levels = new[] { "A1", "A2", "B1", "B1+", "B2" };
        var supported = levels.TakeWhile(level =>
        {
            var group = session.Questions.Where(q => q.Level == level).ToArray();
            return group.Length >= 3 && group.Count(q => IsCorrect(q, session.Responses.GetValueOrDefault(q.Id, ""))) >= Math.Ceiling(group.Length * .67);
        }).LastOrDefault();
        return supported is null ? "Start with A1" : $"{supported} practice starting point";
    }

    public static Lesson Recommend(IReadOnlyList<Lesson> lessons, IReadOnlyList<PracticeSession> completed)
    {
        if (lessons.Count == 0) throw new ArgumentException("At least one lesson is required.", nameof(lessons));
        var weak = completed.Where(s => s.Finished is not null).SelectMany(s =>
            s.Questions.Where(q => !IsCorrect(q, s.Responses.GetValueOrDefault(q.Id, ""))))
            .GroupBy(q => q.Skill).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key;
        var notMastered = lessons.Where(l => !completed.Any(s =>
            s.SourceId == l.Id && s.Mode == "Lesson" && s.Finished is not null && s.Accuracy >= 80)).ToArray();
        return notMastered.FirstOrDefault(l => l.Skill == weak) ?? notMastered.FirstOrDefault() ?? lessons[0];
    }

    public static int Streak(IEnumerable<PracticeSession> sessions, DateTime today)
    {
        var dates = sessions.Where(s => s.Finished is not null)
            .Select(s => s.Finished!.Value.LocalDateTime.Date).ToHashSet();
        var date = today.Date;
        if (!dates.Contains(date)) date = date.AddDays(-1);
        var streak = 0;
        while (dates.Contains(date)) { streak++; date = date.AddDays(-1); }
        return streak;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
    [GeneratedRegex(@"\b[\p{L}\p{N}]+(?:['\u2019-][\p{L}\p{N}]+)*\b")]
    private static partial Regex Words();
    [GeneratedRegex(@"\r?\n\s*\r?\n")]
    private static partial Regex ParagraphBreak();
}
