using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using EnglishB2Academy.Core;

namespace EnglishB2Academy;

public partial class MainWindow : Window
{
    private readonly PreferencesStore preferencesStore = new();
    private Preferences preferences = new();
    private string? startupError;
    private readonly LearningStore store;
    private readonly Dictionary<string, ListBoxItem> navigationItems = new();
    private readonly string[] destinations =
    [
        "Home", "My learning plan", "English courses", "Grammar", "Vocabulary",
        "Reading", "Writing", "Listening", "Speaking", "Examination practice",
        "Mock examinations", "Historical resources", "My mistakes", "My progress", "Examination readiness"
    ];
    private readonly string[] icons =
    [
        "\uE80F", "\uE8FD", "\uE82D", "\uE8D2", "\uE8F1", "\uE736", "\uE70F",
        "\uE767", "\uE720", "\uE9D5", "\uE916", "\uE81C", "\uE7BA", "\uE9D9", "\uE73E"
    ];

    public MainWindow()
    {
        InitializeComponent();
        var workArea = SystemParameters.WorkArea;
        Width = Math.Min(1400, workArea.Width - 32);
        Height = Math.Min(960, workArea.Height - 32);
        Left = workArea.Left + Math.Max(0, (workArea.Width - Width) / 2);
        Top = workArea.Top + Math.Max(0, (workArea.Height - Height) / 2);
        store = new LearningStore(Path.Combine(DataDirectory, "learning.db"));
        try { preferences = preferencesStore.Load(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            startupError = $"Your saved preferences could not be loaded. Defaults are being shown; your file has not been changed.\n{ex.Message}";
        }
        ApplyPreferences();
        for (var index = 0; index < destinations.Length; index++)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new TextBlock
            {
                Text = icons[index], FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 15, Width = 28, VerticalAlignment = VerticalAlignment.Center
            });
            row.Children.Add(new TextBlock { Text = destinations[index], FontSize = 12 });
            var item = new ListBoxItem { Content = row, Tag = destinations[index], ToolTip = destinations[index] };
            System.Windows.Automation.AutomationProperties.SetName(item, destinations[index]);
            navigationItems.Add(destinations[index], item);
            Navigation.Items.Add(item);
        }
        Navigation.SelectedIndex = 0;
        Root.SizeChanged += (_, _) => UpdateLayoutForWidth();
        Loaded += Window_Loaded;
        InitialiseLearning();
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key != System.Windows.Input.Key.F12) return;
            SaveCurrentView();
            e.Handled = true;
        };
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (startupError is not null) ShowMessage("Preferences could not be loaded", startupError);
        Run(() =>
        {
            var backupDirectory = Path.Combine(DataDirectory, "backups");
            Directory.CreateDirectory(backupDirectory);
            var dailyBackup = Path.Combine(backupDirectory, $"auto-{DateTime.Today:yyyyMMdd}.db");
            if (!File.Exists(dailyBackup)) store.Backup(dailyBackup);
        });
        var args = Environment.GetCommandLineArgs();
        var previewIndex = Array.IndexOf(args, "--preview-path");
        if (previewIndex >= 0 && previewIndex + 1 < args.Length)
        {
            await Task.Delay(750);
            SavePreview(args[previewIndex + 1]);
        }
    }

    private void ApplyPreferences()
    {
        var dark = preferences.Theme == "Dark";
        var colours = new Dictionary<string, string>
        {
            ["CanvasBrush"] = dark ? "#111D22" : "#F6F7F9",
            ["CardBrush"] = dark ? "#1C2C31" : "#FFFFFF",
            ["InkBrush"] = dark ? "#EDF5F3" : "#182E33",
            ["MutedBrush"] = dark ? "#A9BCBD" : "#607479",
            ["LineBrush"] = dark ? "#34484B" : "#E3E9E9",
            ["SoftBrush"] = dark ? "#253B3C" : "#EFF5F3",
            ["AccentBrush"] = dark ? "#89D5BE" : "#146C62"
        };
        foreach (var (key, colour) in colours)
            Application.Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colour));
        ReadingScale.ScaleX = ReadingScale.ScaleY = preferences.TextSize == "Large" ? 1.15 : 1;
        GoalValue.Text = preferences.DailyMinutes.ToString();
    }

    private void UpdateLayoutForWidth()
    {
        var compact = Root.ActualWidth < 1180 || preferences.TextSize == "Large";
        SidebarColumn.Width = new GridLength(compact ? 208 : 224);
        Grid.SetRow(SkillsCard, compact ? 1 : 0);
        Grid.SetColumn(SkillsCard, compact ? 0 : 1);
        Grid.SetColumnSpan(TodayCard, compact ? 2 : 1);
        Grid.SetColumnSpan(SkillsCard, compact ? 2 : 1);
        TodayCard.Margin = compact ? new Thickness(0, 0, 0, 16) : new Thickness(0, 0, 10, 0);
        SkillsCard.Margin = compact ? new Thickness(0) : new Thickness(10, 0, 0, 0);
        HeroAside.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Navigation.SelectedItem is ListBoxItem { Tag: string destination }) Run(() => ShowPage(destination));
    }

    private void Navigate(string destination)
    {
        if (navigationItems.TryGetValue(destination, out var item))
        {
            Navigation.SelectedItem = item;
            ShowPage(destination);
        }
    }

    private void ShowPage(string destination)
    {
        FlushDraft();
        if (audio.IsRecording) audio.StopRecording();
        audio.StopSpeech();
        audio.StopPlayback();
        currentDestination = destination;
        clock = null;
        PageTitle.Text = destination;
        HomePage.Visibility = destination == "Home" ? Visibility.Visible : Visibility.Collapsed;
        SecondaryPage.Visibility = destination == "Home" ? Visibility.Collapsed : Visibility.Visible;
        SecondaryPage.Children.Clear();
        ContentScroller.ScrollToTop();
        if (destination == "Home") { RefreshHome(); return; }
        if (destination == "My learning plan") { ShowPlan(); return; }
        if (destination == "Settings & help") { ShowSettings(); return; }
        if (ShowLearningDestination(destination)) return;
        AddHeading(destination, "A place for your next chapter.");
        SecondaryPage.Children.Add(Notice("Foundation stage",
            "This destination is wired into the Windows shell. Its learning functionality is not implemented yet."));
        var descriptions = new Dictionary<string, string>
        {
            ["My progress"] = "No learning records yet. Charts will be based on completed activities, not sample statistics.",
            ["Examination readiness"] = "Not assessed. Readiness requires recent evidence across all skills and multiple unseen timed examinations.",
            ["My mistakes"] = "Nothing recorded yet. Reviewed answers will appear here once exercises are implemented.",
            ["Historical resources"] = "No third-party educational materials have been imported. Every future resource will require verified provenance."
        };
        SecondaryPage.Children.Add(Card(
            Text(descriptions.GetValueOrDefault(destination,
                $"The {destination.ToLowerInvariant()} workspace will be built after this first dashboard design review."), 18),
            Text("No lessons, assessments, examination scores or readiness claims are generated by this shell.", 14, muted: true),
            Action("Back to home", () => Navigate("Home"))));
    }

    private void ShowPlan()
    {
        ShowLearningPlan();
    }

    private void ShowLesson()
    {
        OpenLesson(Grading.Recommend(Curriculum.Lessons, store.Sessions()));
    }

    private void ShowSettings()
    {
        AddHeading("Make this space yours.", "SETTINGS & HELP");
        var theme = new ComboBox { ItemsSource = new[] { "Light", "Dark" }, SelectedItem = preferences.Theme };
        var goal = new ComboBox { ItemsSource = new[] { 10, 20, 30, 45 }, SelectedItem = preferences.DailyMinutes };
        var size = new ComboBox { ItemsSource = new[] { "Standard", "Large" }, SelectedItem = preferences.TextSize };
        System.Windows.Automation.AutomationProperties.SetName(theme, "Appearance");
        System.Windows.Automation.AutomationProperties.SetName(goal, "Daily study goal in minutes");
        System.Windows.Automation.AutomationProperties.SetName(size, "Reading size");
        var status = Text("", 14, muted: true);
        var save = Action("Save preferences", () =>
        {
            var updated = new Preferences
            {
                Theme = (string)theme.SelectedItem,
                DailyMinutes = (int)goal.SelectedItem,
                TextSize = (string)size.SelectedItem
            };
            try
            {
                preferencesStore.Save(updated);
                preferences = updated;
                ApplyPreferences();
                UpdateLayoutForWidth();
                status.Text = "Saved on this Windows account.";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                ShowMessage("Preferences were not saved", ex.Message);
            }
        });
        SecondaryPage.Children.Add(Card(
            Text("Appearance", 14), theme, Text("Daily study goal (minutes)", 14), goal,
            Text("Reading size", 14), size, save, status));
        SecondaryPage.Children.Add(Card(
            Text("Private by design", 20),
            Text("Study records, answers, profile and drafts stay in the local SQLite database. Speech playback uses installed Windows voices. Recordings are local WAV files, created only after microphone confirmation. No account, analytics or cloud API is used.", 14, muted: true),
            Text(PreferencesStore.DefaultFilePath, 12, muted: true),
            Text(DataDirectory, 12, muted: true),
            Text("British English is the default. Practice results are raw local scores, not Cambridge English Scale scores.", 14, muted: true),
            Action("Learner profile", ShowProfile),
            Action("Back up learning database", BackupLearning),
            Action("Restore learning database", RestoreLearning),
            Action("Export learning data (JSON)", ExportLearning),
            Action("Export dashboard screenshot", ExportPreview),
            Action("Delete learning records", DeleteLearning),
            Action("About this build", () => ShowMessage("English B2 Academy · 0.2",
                "Native C# / .NET 10 / WPF with SQLite.\n\nOriginal introductory course library, marked practice, short timed mocks, drafts, local speech and progress tracking. Not a complete curriculum or official exam product. No examination-board affiliation or pass guarantee."))));
    }

    private void AddHeading(string title, string subtitle)
    {
        SecondaryPage.Children.Add(new TextBlock
        {
            Text = subtitle.ToUpperInvariant(), Style = (Style)FindResource("Eyebrow"), Margin = new Thickness(0, 0, 0, 12)
        });
        var heading = Text(title, 30);
        heading.FontWeight = FontWeights.SemiBold;
        heading.Margin = new Thickness(0, 0, 0, 24);
        SecondaryPage.Children.Add(heading);
    }

    private TextBlock Text(string text, double size, bool muted = false) => new()
    {
        Style = (Style)FindResource(muted ? "Muted" : "Body"), Text = text, FontSize = size,
        Margin = new Thickness(0, 0, 0, 16)
    };

    private Border Card(params UIElement[] children)
    {
        var stack = new StackPanel();
        foreach (var child in children) stack.Children.Add(child);
        return new Border { Style = (Style)FindResource("Card"), Child = stack, Margin = new Thickness(0, 0, 0, 22) };
    }

    private Border Notice(string title, string message)
    {
        var stack = new StackPanel();
        var heading = Text(title, 14);
        heading.FontWeight = FontWeights.SemiBold;
        stack.Children.Add(heading);
        var body = Text(message, 14, muted: true);
        body.Margin = new Thickness(0);
        stack.Children.Add(body);
        var border = new Border { CornerRadius = new CornerRadius(10), Padding = new Thickness(20), Child = stack, Margin = new Thickness(0, 0, 0, 22) };
        border.SetResourceReference(Border.BackgroundProperty, "SoftBrush");
        return border;
    }

    private Button Action(string label, System.Action action)
    {
        var button = new Button { Content = label, Style = (Style)FindResource("Primary"), Margin = new Thickness(0, 0, 0, 12) };
        button.Click += (_, _) => Run(action);
        return button;
    }

    private void ShowMessage(string title, string message) => MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    private async void ExportPreview()
    {
        var picker = new SaveFileDialog { Filter = "PNG image (*.png)|*.png", FileName = "01-home-dashboard.png", Title = "Save actual dashboard screenshot" };
        if (picker.ShowDialog(this) != true) return;
        Navigate("Home");
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        await Task.Delay(150);
        SavePreview(picker.FileName);
    }

    private bool SavePreview(string path)
    {
        try
        {
            Root.UpdateLayout();
            var dpi = VisualTreeHelper.GetDpi(Root);
            var bitmap = new RenderTargetBitmap(
                (int)Math.Ceiling(Root.ActualWidth * dpi.DpiScaleX),
                (int)Math.Ceiling(Root.ActualHeight * dpi.DpiScaleY),
                dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
            bitmap.Render(Root);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var fullPath = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            using var stream = File.Create(fullPath);
            encoder.Save(stream);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            ShowMessage("Preview could not be saved", ex.Message);
            return false;
        }
    }

    private void SaveView_Click(object sender, RoutedEventArgs e) => SaveCurrentView();

    private void SaveCurrentView()
    {
        var title = string.Concat(PageTitle.Text.Select(c => char.IsLetterOrDigit(c) ? c : '-'));
        var path = Path.Combine(DataDirectory, "previews", $"{DateTime.Now:yyyyMMdd-HHmmssfff}-{title}.png");
        if (SavePreview(path))
        {
            HeaderStatus.Text = "VIEW SAVED";
            HeaderStatus.ToolTip = path;
        }
    }

    private void Plan_Click(object sender, RoutedEventArgs e) => Run(() => Navigate("My learning plan"));
    private void Lesson_Click(object sender, RoutedEventArgs e) => Run(ShowLesson);
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        Navigation.SelectedItem = null;
        Run(() => ShowPage("Settings & help"));
    }
    private void Skill_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string skill }) Run(() => Navigate(skill));
    }
}
