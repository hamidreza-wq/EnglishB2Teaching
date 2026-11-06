using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using EnglishB2Academy.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;

namespace EnglishB2Academy;

public partial class MainWindow
{
    private static string DataDirectory
    {
        get
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, "--data-directory");
            return index >= 0 && index + 1 < args.Length ? Path.GetFullPath(args[index + 1]) :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EnglishB2Academy");
        }
    }

    private readonly LocalAudio audio = new();
    private readonly DispatcherTimer learningTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private PracticeSession? currentSession;
    private TextBlock? clock;
    private Draft? pendingDraft;
    private DateTimeOffset lastDraftSave = DateTimeOffset.UtcNow;
    private string? currentDestination;

    private void InitialiseLearning()
    {
        foreach (var session in store.Sessions().Where(s => s.Finished is null && s.IsExpired(DateTimeOffset.UtcNow)))
            store.SaveSession(session with { Finished = session.Deadline });
        learningTimer.Tick += (_, _) => Run(() =>
        {
            if (pendingDraft is not null && DateTimeOffset.UtcNow - lastDraftSave >= TimeSpan.FromSeconds(3))
                FlushDraft();
            if (currentSession is { Finished: null } session)
            {
                if (IsActive && currentDestination == "session") session.ActiveSeconds++;
                if (clock is not null)
                {
                    var remaining = session.Deadline - DateTimeOffset.UtcNow;
                    clock.Text = remaining is { } time
                        ? $"Time remaining: {Math.Max(0, (int)time.TotalMinutes):00}:{Math.Max(0, time.Seconds):00}  |  Answers saved locally"
                        : $"Question {session.Position + 1} of {session.Questions.Count}  |  Untimed practice";
                }
                if (session.ActiveSeconds % 15 == 0) store.SaveSession(session);
                if (session.IsExpired(DateTimeOffset.UtcNow)) CompleteSession(false);
            }
        });
        learningTimer.Start();
        Closing += (_, e) =>
        {
            try
            {
                FlushDraft();
                if (currentSession is { Finished: null }) store.SaveSession(currentSession);
                audio.Dispose();
            }
            catch (Exception ex) when (IsExpected(ex))
            {
                e.Cancel = true;
                ShowMessage("Unable to close safely", $"Your latest work or recording could not be saved. Resolve the error and try closing again.\n{ex.Message}");
                return;
            }
            learningTimer.Stop();
            store.Dispose();
        };
        RefreshHome();
    }

    private static bool IsExpected(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or SqliteException or JsonException or
            InvalidOperationException or ArgumentException or System.Runtime.InteropServices.COMException;

    private void Run(System.Action action)
    {
        try { action(); }
        catch (Exception ex) when (IsExpected(ex))
        {
            ShowMessage("Action could not be completed", ex.Message);
        }
    }

    private void BeginPage(string title)
    {
        FlushDraft();
        if (audio.IsRecording) audio.StopRecording();
        audio.StopSpeech();
        audio.StopPlayback();
        currentDestination = title;
        clock = null;
        Navigation.SelectedItem = null;
        HomePage.Visibility = Visibility.Collapsed;
        SecondaryPage.Visibility = Visibility.Visible;
        SecondaryPage.Children.Clear();
        PageTitle.Text = title;
        ContentScroller.ScrollToTop();
    }

    private bool ShowLearningDestination(string destination)
    {
        FlushDraft();
        audio.StopSpeech();
        audio.StopPlayback();
        currentDestination = destination;
        switch (destination)
        {
            case "English courses": ShowCourses(); break;
            case "Grammar": ShowCourses("Grammar"); break;
            case "Vocabulary": ShowVocabulary(); break;
            case "Reading": ShowCourses("Reading"); break;
            case "Listening": ShowCourses("Listening"); break;
            case "Writing": ShowWriting(); break;
            case "Speaking": ShowSpeaking(); break;
            case "Examination practice": ShowExams(false); break;
            case "Mock examinations": ShowExams(true); break;
            case "My mistakes": ShowMistakes(); break;
            case "My progress": ShowProgress(); break;
            case "Examination readiness": ShowReadiness(); break;
            case "Historical resources":
                AddHeading("Resources with a traceable origin", "CONTENT & LICENSING");
                SecondaryPage.Children.Add(Notice("No imported examination papers",
                    "This release uses original practice content only. No historical papers are bundled or claimed to be public domain."));
                SecondaryPage.Children.Add(Card(Text(
                    "The local lesson and practice library was authored for this project. AI assistance is documented; it does not imply public-domain status. No examination-board endorsement is claimed.", 16),
                    Text("Sources for factual examination structure are shown in Examination practice. Original questions are independent of official papers.", 14, muted: true)));
                break;
            default: return false;
        }
        return true;
    }

    private void RefreshHome()
    {
        var sessions = store.Sessions();
        var completed = sessions.Where(s => s.Finished is not null).ToList();
        var recommended = Grading.Recommend(Curriculum.Lessons, completed);
        RecommendationTitle.Text = recommended.Title;
        RecommendationDetail.Text = $"{recommended.Level} · {recommended.Skill} · Original lesson with {recommended.Questions.Length} marked questions. Continue at your own pace.";
        var diagnostic = completed.FirstOrDefault(s => s.Mode == "Diagnostic");
        LevelValue.Text = diagnostic is null ? "Not assessed" : Grading.DiagnosticLabel(diagnostic);
        var mastered = Curriculum.Lessons.Count(l => completed.Any(s => s.SourceId == l.Id && s.Mode == "Lesson" && s.Accuracy >= 80));
        ActivitySummary.Text = $"{completed.Count} completed activities · {Grading.Streak(completed, DateTime.Today)}-day activity streak";
        ProgressSummary.Text = $"{mastered}/{Curriculum.Lessons.Count} lesson checks at 80%+. This indicates practice success, not full skill mastery.";
    }

    private void ShowLearningPlan()
    {
        AddHeading("Small steps. Lasting confidence.", "YOUR LEARNING PLAN");
        var sessions = store.Sessions();
        var profile = store.Profile();
        SecondaryPage.Children.Add(Card(
            Text("Your starting point", 22),
            Text($"Self-reported level: {profile.KnownLevel}. Daily preference: {preferences.DailyMinutes} minutes. Pace: {profile.Pace}.", 14, muted: true),
            Text("The 15-question diagnostic samples grammar and vocabulary only. It cannot certify CEFR proficiency or assess writing and speaking.", 14, muted: true),
            Action("Take starting-point diagnostic", () => StartSession(PracticeFactory.Diagnostic(DateTimeOffset.UtcNow))),
            Action("Edit learner profile", ShowProfile)));
        foreach (var session in sessions.Where(s => s.Finished is null))
        {
            var saved = session;
            SecondaryPage.Children.Add(Card(Text($"In progress: {session.Title}", 18),
                Text(session.Deadline is { } deadline ? $"Deadline: {deadline.LocalDateTime:g}. The timer continues while the app is closed." : "Untimed. Continue where you left off.", 14, muted: true),
                Action("Resume saved activity", () => ResumeSession(saved))));
        }
        var next = Grading.Recommend(Curriculum.Lessons, sessions);
        SecondaryPage.Children.Add(Card(Text("Recommended next lesson", 22), Text($"{next.Level} · {next.Title}", 18),
            Text("Recommendations prioritise skills with recorded mistakes, then lessons without an 80% practice result. A repeated question is not evidence of unseen exam readiness.", 14, muted: true),
            Action("Start recommended lesson", () => OpenLesson(next)),
            Action("Browse all courses", () => Navigate("English courses"))));
    }

    private void ShowCourses(string? skill = null)
    {
        AddHeading(skill is null ? "A course for your next step" : $"{skill}, step by step", "ORIGINAL A1-B2 LIBRARY");
        SecondaryPage.Children.Add(Notice("Introductory course collection",
            $"{Curriculum.Lessons.Count} short lessons and {Curriculum.Lessons.Sum(l => l.Questions.Length)} questions across five levels. This is a usable starting library, not the complete CEFR curriculum."));
        var search = new TextBox { Margin = new Thickness(0, 0, 0, 12), Padding = new Thickness(12, 8, 12, 8) };
        System.Windows.Automation.AutomationProperties.SetName(search, "Search lesson title or level");
        var level = new ComboBox { ItemsSource = new[] { "All levels" }.Concat(Curriculum.Levels), SelectedIndex = 0 };
        System.Windows.Automation.AutomationProperties.SetName(level, "Filter level");
        SecondaryPage.Children.Add(Text("Search title, skill or level", 14));
        SecondaryPage.Children.Add(search);
        SecondaryPage.Children.Add(level);
        var list = new StackPanel();
        SecondaryPage.Children.Add(list);
        void Render()
        {
            list.Children.Clear();
            var selected = level.SelectedItem as string;
            var completed = store.Sessions();
            var matches = Curriculum.Lessons.Where(l => (skill is null || l.Skill == skill) &&
                (selected == "All levels" || l.Level == selected) &&
                $"{l.Level} {l.Title} {l.Skill}".Contains(search.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0) list.Children.Add(Text("No lessons match this search.", 16));
            foreach (var lesson in matches)
            {
                var passed = completed.Any(s => s.SourceId == lesson.Id && s.Mode == "Lesson" && s.Finished is not null && s.Accuracy >= 80);
                list.Children.Add(Card(Text($"{lesson.Level}  /  {lesson.Skill.ToUpperInvariant()}", 12, muted: true),
                    Text(lesson.Title, 22), Text(passed ? "Practice check passed (80%+)." : "Learn, then check your understanding.", 14, muted: true),
                    Action("Open lesson", () => OpenLesson(lesson))));
            }
        }
        search.TextChanged += (_, _) => Run(Render);
        level.SelectionChanged += (_, _) => Run(Render);
        Render();
    }

    private void OpenLesson(Lesson lesson)
    {
        BeginPage(lesson.Title);
        AddHeading(lesson.Title, $"{lesson.Level} / {lesson.Skill} / ORIGINAL LESSON");
        SecondaryPage.Children.Add(Card(Text("Understand", 22), Text(lesson.Explanation, 17),
            Text("Example / source text", 18), Text(lesson.Example, 18),
            Text("Watch out", 18), Text(lesson.CommonMistake, 15, muted: true),
            Action("Listen to the example", () => audio.Speak(lesson.Example)),
            Action($"Practise · {lesson.Questions.Length} questions", () => StartSession(PracticeFactory.Lesson(lesson))),
            Action("Back to courses", () => Navigate("English courses"))));
    }

    private void ShowProfile()
    {
        BeginPage("Learner profile");
        AddHeading("Plan around your life", "LOCAL PROFILE");
        var profile = store.Profile();
        ComboBox Pick(string name, string[] choices, string selected)
        {
            var pick = new ComboBox { ItemsSource = choices, SelectedItem = selected };
            System.Windows.Automation.AutomationProperties.SetName(pick, name);
            return pick;
        }
        var level = Pick("Known level", ["Not sure", "A1", "A2", "B1", "B1+", "B2", "C1", "C2"], profile.KnownLevel);
        var language = new TextBox { Text = profile.FirstLanguage, MaxLength = 100, Padding = new Thickness(8), Margin = new Thickness(0, 0, 0, 16) };
        System.Windows.Automation.AutomationProperties.SetName(language, "First language optional");
        var variety = Pick("English variety", ["British English", "American English"], profile.Variety);
        var pace = Pick("Learning pace", ["Gentle", "Balanced", "Intensive"], profile.Pace);
        var date = new DatePicker { Margin = new Thickness(0, 0, 0, 16) };
        if (DateTime.TryParse(profile.ExamDate, out var parsed)) date.SelectedDate = parsed;
        System.Windows.Automation.AutomationProperties.SetName(date, "Target exam date optional");
        SecondaryPage.Children.Add(Card(Text("Target: Cambridge B2 First", 20),
            Text("Other board-specific pathways require separate content and scoring and are not implemented. Variety is a preference; original content currently uses British English.", 14, muted: true),
            Text("Existing level (self-reported)", 14), level,
            Text("First language (optional; assistance not yet available)", 14), language,
            Text("Preferred English variety", 14), variety, Text("Pace", 14), pace,
            Text("Target date (optional)", 14), date,
            Action("Save profile", () =>
            {
                if (date.SelectedDate is { } target && target.Date < DateTime.Today)
                    throw new ArgumentException("Choose today or a future target date.");
                store.SaveProfile(new LearnerProfile((string)level.SelectedItem, language.Text.Trim(),
                    (string)variety.SelectedItem, date.SelectedDate?.ToString("yyyy-MM-dd") ?? "", (string)pace.SelectedItem));
                Navigate("My learning plan");
            })));
    }

    private void BackupLearning()
    {
        FlushDraft();
        var picker = new SaveFileDialog { Filter = "SQLite backup (*.db)|*.db", FileName = $"EnglishB2-{DateTime.Now:yyyyMMdd-HHmmss}.db" };
        if (picker.ShowDialog(this) != true) return;
        store.Backup(picker.FileName);
        ShowMessage("Backup saved", "The learning database was backed up. Appearance preferences and WAV recordings are separate and are not included.");
    }

    private void RestoreLearning()
    {
        var picker = new OpenFileDialog { Filter = "SQLite backup (*.db)|*.db" };
        if (picker.ShowDialog(this) != true) return;
        LearningStore.ValidateBackup(picker.FileName);
        if (MessageBox.Show(this, "Replace the learning database with this backup? A safety backup will be kept. Recordings and appearance preferences will not change.",
            "Restore learning data", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        FlushDraft();
        store.Backup(Path.Combine(DataDirectory, $"before-restore-{Guid.NewGuid():N}.db"));
        store.Restore(picker.FileName);
        currentSession = null;
        Navigate("Home");
        ShowMessage("Restored", "Learning data has been restored. Any expired timed activity will be submitted when resumed.");
    }

    private void ExportLearning()
    {
        FlushDraft();
        var picker = new SaveFileDialog { Filter = "JSON export (*.json)|*.json", FileName = "EnglishB2-learning-data.json" };
        if (picker.ShowDialog(this) != true) return;
        File.WriteAllText(picker.FileName, store.ExportJson());
        ShowMessage("Export saved", "Your learning records and drafts were exported locally. This file may contain your written answers. It does not contain voice recordings.");
    }

    private void DeleteLearning()
    {
        if (MessageBox.Show(this, "Permanently remove all learning attempts, drafts, profile and vocabulary review schedules? This does not remove recordings, exported files or backups.",
            "Delete learning records", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        pendingDraft = null;
        currentSession = null;
        store.DeleteLearningRecords();
        Navigate("Home");
    }
}
