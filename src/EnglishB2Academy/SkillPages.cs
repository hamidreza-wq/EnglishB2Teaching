using System.IO;
using System.Windows;
using System.Windows.Controls;
using EnglishB2Academy.Core;

namespace EnglishB2Academy;

public partial class MainWindow
{
    private void ShowVocabulary()
    {
        AddHeading("Make useful words your own", "VOCABULARY & SPACED REVIEW");
        var cards = store.ReviewCards();
        var due = Curriculum.Vocabulary.Where(v =>
            cards.SingleOrDefault(c => c.Word == v.Word)?.Due <= DateTimeOffset.UtcNow ||
            cards.All(c => c.Word != v.Word)).ToArray();
        SecondaryPage.Children.Add(Card(
            Text($"{due.Length} cards ready · {Curriculum.Vocabulary.Count} original entries", 22),
            Text("Recall schedules start at 1 day, then double up to 60 days. 'Again' brings a card back in 10 minutes. A schedule is not proof of vocabulary mastery.", 14, muted: true),
            Action("Review due flashcards", () =>
            {
                if (due.Length == 0) { ShowMessage("All caught up", "No cards are due yet. Browse the vocabulary lessons below."); return; }
                ShowFlashcard(due, 0);
            }),
            Action("Open vocabulary lessons", () =>
            {
                BeginPage("Vocabulary lessons");
                ShowCourses("Vocabulary");
            })));
        foreach (var item in Curriculum.Vocabulary)
            SecondaryPage.Children.Add(Card(Text(item.Word, 21), Text(item.Meaning, 15),
                Text(item.Example, 15, muted: true), Action("Hear word and example", () => audio.Speak($"{item.Word}. {item.Example}"))));
    }

    private void ShowFlashcard(VocabularyItem[] cards, int index)
    {
        BeginPage("Flashcards");
        if (index >= cards.Length)
        {
            AddHeading("Review complete", "VOCABULARY");
            SecondaryPage.Children.Add(Card(Text("Your recall choices have been saved.", 20),
                Action("Back to vocabulary", () => Navigate("Vocabulary"))));
            return;
        }
        var item = cards[index];
        AddHeading(item.Word, $"CARD {index + 1} OF {cards.Length}");
        var reveal = new StackPanel { Visibility = Visibility.Collapsed };
        reveal.Children.Add(Text(item.Meaning, 20));
        reveal.Children.Add(Text(item.Example, 17, muted: true));
        void Review(bool recalled)
        {
            store.Review(item.Word, recalled, DateTimeOffset.UtcNow);
            ShowFlashcard(cards, index + 1);
        }
        reveal.Children.Add(Action("Again · 10 minutes", () => Review(false)));
        reveal.Children.Add(Action("I recalled it · schedule next review", () => Review(true)));
        SecondaryPage.Children.Add(Card(Text("Recall its meaning and make a sentence before revealing.", 16),
            Action("Reveal meaning", () => reveal.Visibility = Visibility.Visible),
            Action("Hear pronunciation", () => audio.Speak(item.Word)), reveal));
    }

    private void ShowWriting()
    {
        AddHeading("Write with purpose", "WRITING WORKSHOP");
        SecondaryPage.Children.Add(Notice("Original prompts, not automatic grading",
            "Drafts save locally. Feedback checks word count and paragraph breaks only; it cannot judge meaning, language quality or award an examination score."));
        foreach (var (prompt, index) in Curriculum.WritingPrompts.Select((p, i) => (p, i)))
        {
            var id = $"writing-{index}";
            var existing = store.Drafts().FirstOrDefault(d => d.Id == id);
            SecondaryPage.Children.Add(Card(Text(prompt, 17),
                Action(existing is null ? "Start draft" : "Continue saved draft", () =>
                    OpenDraft(existing ?? new Draft(id, "Writing", prompt, "", DateTimeOffset.UtcNow)))));
        }
        foreach (var draft in store.Drafts().Where(d => d.Kind == "Timed writing"))
            SecondaryPage.Children.Add(Card(Text($"Saved timed-writing draft · {draft.Updated.LocalDateTime:g}", 17),
                Action("Review draft", () => OpenDraft(draft))));
        SecondaryPage.Children.Add(Card(Text("Timed writing practice · 80 minutes", 22),
            Text("Two original tasks. Drafts are auto-saved. A local deadline locks the editor on expiry; there is no automated score. The deadline is preserved when the app is closed.", 14, muted: true),
            Action("Start timed writing pair", StartTimedWriting)));
    }

    private void StartTimedWriting()
    {
        var id = "timed-writing-" + Guid.NewGuid().ToString("N");
        var deadline = DateTimeOffset.UtcNow.AddMinutes(80);
        var prompt = $"Deadline (UTC): {deadline:O}\n\nTASK 1\n{Curriculum.WritingPrompts[0]}\n\nTASK 2\n{Curriculum.WritingPrompts[2]}\n\nUse a clear TASK 2 heading to separate your answers.";
        var draft = new Draft(id, "Timed writing", prompt, "", DateTimeOffset.UtcNow);
        store.SaveDraft(draft);
        OpenDraft(draft);
    }

    private void OpenDraft(Draft draft)
    {
        BeginPage("Writing editor");
        AddHeading(draft.Kind, "LOCAL DRAFT");
        SecondaryPage.Children.Add(Card(Text(draft.Prompt, 17)));
        var editor = new TextBox
        {
            Text = draft.Text, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 300,
            MaxLength = 20000, Padding = new Thickness(16), VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 0, 16), FontSize = 17
        };
        System.Windows.Automation.AutomationProperties.SetName(editor, "Writing draft");
        var feedback = Text(Grading.WritingFeedback(draft.Text), 14, muted: true);
        DateTimeOffset? deadline = null;
        if (draft.Kind == "Timed writing")
        {
            var firstLine = draft.Prompt.Split('\n')[0].Replace("Deadline (UTC): ", "");
            if (!DateTimeOffset.TryParse(firstLine, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                throw new InvalidDataException("This timed draft has an invalid deadline.");
            deadline = parsed;
        }
        var timerLabel = Text("Drafts auto-save after at most three seconds and when leaving the page.", 14, muted: true);
        editor.TextChanged += (_, _) =>
        {
            if (deadline is { } end && DateTimeOffset.UtcNow >= end)
            {
                editor.IsReadOnly = true;
                return;
            }
            pendingDraft = draft with { Text = editor.Text, Updated = DateTimeOffset.UtcNow };
            feedback.Text = draft.Kind == "Timed writing" ?
                $"{Grading.WordCount(editor.Text)} words in total. Check each task independently against 140-190 words." :
                Grading.WritingFeedback(editor.Text);
        };
        if (deadline is { } expires)
        {
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (_, _) => Run(() =>
            {
                if (currentDestination != "Writing editor") { timer.Stop(); return; }
                var remaining = expires - DateTimeOffset.UtcNow;
                timerLabel.Text = remaining > TimeSpan.Zero ? $"Time left: {(int)remaining.TotalMinutes:00}:{remaining.Seconds:00} · Draft auto-saves." :
                    "Time is up. This draft is read-only. You can review or export it.";
                if (remaining <= TimeSpan.Zero) { editor.IsReadOnly = true; FlushDraft(); timer.Stop(); }
            });
            editor.IsReadOnly = DateTimeOffset.UtcNow >= expires;
            timer.Start();
        }
        SecondaryPage.Children.Add(Card(timerLabel, editor, feedback,
            Action("Save draft now", () =>
            {
                FlushDraft();
                if (!editor.IsReadOnly)
                    store.SaveDraft(draft with { Text = editor.Text, Updated = DateTimeOffset.UtcNow });
                timerLabel.Text = "Draft saved locally.";
            }),
            Action("Hear my draft", () => audio.Speak(editor.Text)),
            Action("Back to writing", () => Navigate("Writing"))));
        SecondaryPage.Children.Add(Card(Text("Self-review checklist", 20),
            Text("Have I answered every part of the task?\nDoes each paragraph have a purpose?\nHave I supported my ideas with examples?\nIs my register appropriate for the reader?\nHave I checked tenses, agreement, articles and spelling?", 16)));
    }

    private void FlushDraft()
    {
        if (pendingDraft is null) return;
        store.SaveDraft(pendingDraft);
        pendingDraft = null;
        lastDraftSave = DateTimeOffset.UtcNow;
    }

    private void ShowSpeaking()
    {
        AddHeading("Give your ideas a voice", "SPEAKING STUDIO");
        SecondaryPage.Children.Add(Notice("Local recording with your permission",
            "Recording starts only after confirmation and stops after two minutes or when you leave this workspace. No speech recognition, pronunciation score or cloud transcription is used."));
        var status = Text("Microphone idle. You can practise aloud without recording.", 16);
        var recordingTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        var started = DateTimeOffset.MinValue;
        void Stop()
        {
            if (!audio.IsRecording) return;
            var path = audio.StopRecording();
            recordingTimer.Stop();
            status.Text = $"Saved locally: {Path.GetFileName(path)}. Reopen Speaking to see it in the recordings list.";
        }
        recordingTimer.Tick += (_, _) => Run(() =>
        {
            if (!audio.IsRecording) { recordingTimer.Stop(); return; }
            var seconds = (int)(DateTimeOffset.UtcNow - started).TotalSeconds;
            status.Text = $"RECORDING · {seconds}s / 120s · Stop and save when ready.";
            if (seconds >= 120) Stop();
        });
        SecondaryPage.Children.Add(Card(status,
            Action("Start microphone recording", () =>
            {
                if (audio.IsRecording) throw new InvalidOperationException("Stop the current recording first.");
                if (MessageBox.Show(this, "Allow this app to record your microphone now? A local WAV file will be saved for this Windows account. Nothing is uploaded.",
                    "Microphone permission", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                var folder = Path.Combine(DataDirectory, "recordings");
                Directory.CreateDirectory(folder);
                audio.StartRecording(Path.Combine(folder, $"speaking-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.wav"));
                started = DateTimeOffset.UtcNow;
                status.Text = "RECORDING · Microphone is active.";
                recordingTimer.Start();
            }),
            Action("Stop & save recording", Stop)));
        foreach (var prompt in Curriculum.SpeakingPrompts)
            SecondaryPage.Children.Add(Card(Text(prompt, 17),
                Action("Hear prompt", () => audio.Speak(prompt))));
        SecondaryPage.Children.Add(Card(Text("Self-review, not automated scoring", 20),
            Text("Listen for clear organisation, relevant detail, understandable pronunciation, varied grammar and vocabulary. With a partner, also reflect on turn-taking. A recording alone does not establish B2 proficiency.", 16)));
        var recordings = Path.Combine(DataDirectory, "recordings");
        if (!Directory.Exists(recordings)) return;
        foreach (var file in Directory.GetFiles(recordings, "speaking-*.wav").OrderByDescending(Path.GetFileName))
        {
            var path = file;
            SecondaryPage.Children.Add(Card(Text(Path.GetFileName(file), 15),
                Action("Play recording", () => audio.PlayRecording(path)),
                Action("Stop playback", audio.StopPlayback),
                Action("Delete this recording", () =>
                {
                    if (MessageBox.Show(this, $"Permanently delete {Path.GetFileName(path)}?", "Delete recording",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                    audio.StopPlayback();
                    File.Delete(path);
                    Navigate("Speaking");
                })));
        }
    }
}
