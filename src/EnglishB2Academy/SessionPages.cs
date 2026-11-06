using System.Windows;
using System.Windows.Controls;
using EnglishB2Academy.Core;

namespace EnglishB2Academy;

public partial class MainWindow
{
    private void StartSession(PracticeSession session)
    {
        if (currentSession is { Finished: null })
            store.SaveSession(currentSession);
        store.SaveSession(session);
        currentSession = session;
        RenderQuestion();
    }

    private void ResumeSession(PracticeSession session)
    {
        currentSession = session;
        if (session.Finished is not null) { ShowResults(session); return; }
        if (session.IsExpired(DateTimeOffset.UtcNow)) { CompleteSession(false); return; }
        RenderQuestion();
    }

    private void RenderQuestion()
    {
        if (currentSession is not { Finished: null } session) return;
        if (session.IsExpired(DateTimeOffset.UtcNow)) { CompleteSession(false); return; }
        BeginPage("Practice");
        currentDestination = "session";
        AddHeading(session.Title, $"{session.Mode.ToUpperInvariant()} / {session.Position + 1} OF {session.Questions.Count}");
        clock = Text(session.Deadline is null ? "Untimed practice" : $"Deadline: {session.Deadline.Value.LocalDateTime:T}", 16);
        SecondaryPage.Children.Add(clock);
        if (session.Mode == "Exam")
            SecondaryPage.Children.Add(Notice("Timed original practice",
                "No hints or answer key before submission. Answers and deadline are saved locally. This is a short practice sample, not a full official paper. Leaving the page or closing the app does not pause the deadline."));
        var question = session.Questions[session.Position];
        if (question.Passage.Length > 0) SecondaryPage.Children.Add(Card(Text(question.Passage, 17)));
        if (question.Audio.Length > 0)
        {
            var used = session.AudioPlays.GetValueOrDefault(question.Audio);
            var play = Action(session.Mode == "Exam" ? $"Play recording ({Math.Max(0, 2 - used)} plays left for this clip)" : "Play local speech audio", () =>
            {
                if (session.Mode == "Exam" && session.AudioPlays.GetValueOrDefault(question.Audio) >= 2)
                    throw new InvalidOperationException("This clip has already been played twice. Continue with the questions.");
                audio.Speak(question.Audio);
                session.AudioPlays[question.Audio] = session.AudioPlays.GetValueOrDefault(question.Audio) + 1;
                store.SaveSession(session);
            });
            SecondaryPage.Children.Add(Card(Text("Listen before answering. Shared clips can cover several questions.", 14, muted: true), play,
                Action("Stop audio", audio.StopSpeech)));
            if (session.Mode != "Exam")
            {
                var transcript = Text(question.Audio, 16);
                transcript.Visibility = Visibility.Collapsed;
                SecondaryPage.Children.Add(Action("Show / hide transcript", () =>
                    transcript.Visibility = transcript.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible));
                SecondaryPage.Children.Add(transcript);
            }
        }
        var panel = new StackPanel();
        panel.Children.Add(Text(question.Prompt, 22));
        if (question.Options.Length > 0)
        {
            foreach (var option in question.Options)
            {
                var choice = new RadioButton
                {
                    Content = Text(option, 16), IsChecked = session.Responses.GetValueOrDefault(question.Id) == option,
                    GroupName = session.Id + question.Id, Margin = new Thickness(0, 0, 0, 12)
                };
                choice.Checked += (_, _) => Run(() => SaveResponse(session, question, option));
                panel.Children.Add(choice);
            }
        }
        else
        {
            var answer = new TextBox
            {
                Text = session.Responses.GetValueOrDefault(question.Id, ""), MaxLength = 500,
                Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 16)
            };
            System.Windows.Automation.AutomationProperties.SetName(answer, "Your answer");
            answer.TextChanged += (_, _) => Run(() => SaveResponse(session, question, answer.Text));
            panel.Children.Add(answer);
        }
        SecondaryPage.Children.Add(Card(panel));
        var buttons = new WrapPanel();
        if (session.Position > 0) buttons.Children.Add(Action("Previous", () =>
        {
            session.Position--;
            store.SaveSession(session);
            RenderQuestion();
        }));
        if (session.Position + 1 < session.Questions.Count) buttons.Children.Add(Action("Save & next", () =>
        {
            session.Position++;
            store.SaveSession(session);
            RenderQuestion();
        }));
        buttons.Children.Add(Action("Submit activity", () => CompleteSession(true)));
        SecondaryPage.Children.Add(buttons);
        SecondaryPage.Children.Add(Text("Unanswered questions count as incorrect. Practice records are local; no official scale conversion is applied.", 13, muted: true));
    }

    private void SaveResponse(PracticeSession session, Question question, string response)
    {
        if (session.Finished is not null) return;
        if (session.IsExpired(DateTimeOffset.UtcNow)) { CompleteSession(false); return; }
        session.Responses[question.Id] = response;
        store.SaveSession(session);
    }

    private void CompleteSession(bool confirm)
    {
        if (currentSession is not { Finished: null } session) return;
        if (confirm && MessageBox.Show(this, "Submit your answers now? Unanswered questions count as incorrect. The answer review will be shown after submission.",
            "Submit activity", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        var now = DateTimeOffset.UtcNow;
        var finished = session with { Finished = session.IsExpired(now) ? session.Deadline : now };
        store.SaveSession(finished);
        currentSession = finished;
        audio.StopSpeech();
        ShowResults(finished);
    }

    private void ShowResults(PracticeSession session)
    {
        BeginPage("Activity results");
        AddHeading(session.Title, "SAVED LOCAL RESULT");
        SecondaryPage.Children.Add(Card(
            Text($"{session.Correct} / {session.Questions.Count} correct  ·  {session.Accuracy:0}%", 28),
            Text("Exact-answer raw practice score. Each question is worth one point. No partial credit or Cambridge English Scale score is claimed.", 14, muted: true),
            Text(session.Mode == "Diagnostic" ? $"{Grading.DiagnosticLabel(session)}. This is a small grammar/vocabulary check, not a CEFR placement assessment." :
                session.Mode == "Lesson" && session.Accuracy >= 80 ? "Practice check passed. Revisit later and use unseen tasks to test lasting understanding." : "Use the explanations below to plan your next practice.", 16),
            Action("Continue learning", ShowLesson),
            Action("My progress", () => Navigate("My progress"))));
        foreach (var question in session.Questions)
        {
            var answer = session.Responses.GetValueOrDefault(question.Id, "");
            var correct = Grading.IsCorrect(question, answer);
            SecondaryPage.Children.Add(Card(Text($"{(correct ? "Correct" : "Review")} · {question.Prompt}", 18),
                Text($"Your answer: {(answer.Length == 0 ? "(unanswered)" : answer)}", 15),
                Text($"Accepted answer: {string.Join(" / ", question.Answers)}", 15),
                Text(question.Explanation, 15, muted: true)));
        }
    }

    private void ShowExams(bool mock)
    {
        AddHeading(mock ? "Practise under a deadline" : "Understand your examination", "EXAMINATION CENTRE");
        var profile = Curriculum.Cambridge;
        SecondaryPage.Children.Add(Notice("Independent practice, not an official examination",
            "This release contains a short 30-question mock (22 reading/language + 8 listening). It does not implement a complete 52-question / 30-question official-format mock. Writing and speaking are separate unscored practice workspaces."));
        SecondaryPage.Children.Add(Card(Text("Short original practice mock · 45 minutes", 22),
            Text("30 objective questions; answers hidden until submission; shared listening clips limited to two plays. Timer persists across closing. Local speech is synthetic, not an exam-board recording.", 15, muted: true),
            Action("Start 45-minute short mock", () => StartSession(PracticeFactory.Exam("Mock", DateTimeOffset.UtcNow))),
            Action("Reading & language · 22 questions / 30 minutes", () => StartSession(PracticeFactory.Exam("Reading", DateTimeOffset.UtcNow))),
            Action("Listening · 8 questions / 15 minutes", () => StartSession(PracticeFactory.Exam("Listening", DateTimeOffset.UtcNow))),
            Action("Writing practice", () => Navigate("Writing")),
            Action("Speaking practice", () => Navigate("Speaking"))));
        AddHeading("Official structure reference", profile.Name);
        foreach (var paper in profile.Papers)
            SecondaryPage.Children.Add(Card(Text($"{paper.Name} · approximately {paper.Minutes} minutes", 20),
                Text($"{paper.Parts} parts" + (paper.Questions > 0 ? $" · {paper.Questions} tasks/questions" : ""), 14, muted: true),
                Text(paper.Instructions, 15)));
        SecondaryPage.Children.Add(Card(Text($"Factual structure checked: {profile.Verified}", 14),
            Text(profile.Source, 13, muted: true),
            Text("The official timings above describe the real examination, not the shortened activities provided here. No board-specific score conversion is implemented. Trinity and LANGUAGECERT pathways are not yet available.", 14, muted: true)));
        var incomplete = store.Sessions().Where(s => s.Mode == "Exam" && s.Finished is null).ToArray();
        foreach (var saved in incomplete)
            SecondaryPage.Children.Add(Card(Text($"Saved: {saved.Title}", 18),
                Action("Resume timed practice", () => ResumeSession(saved))));
    }
}
