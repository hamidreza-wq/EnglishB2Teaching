using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;

namespace EnglishB2Academy;

internal sealed class LocalAudio : IDisposable
{
    private SpeechSynthesizer? synthesizer;
    private bool recording;
    private string? recordingPath;
    public bool IsRecording => recording;

    public void Speak(string text)
    {
        synthesizer ??= new SpeechSynthesizer();
        var british = synthesizer.GetInstalledVoices()
            .FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.Name == "en-GB");
        if (british is not null) synthesizer.SelectVoice(british.VoiceInfo.Name);
        else
        {
            var english = synthesizer.GetInstalledVoices().FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.TwoLetterISOLanguageName == "en")
                ?? throw new InvalidOperationException("No English speech voice is installed. Install an English Windows voice or use the transcript in untimed lessons.");
            synthesizer.SelectVoice(english.VoiceInfo.Name);
        }
        synthesizer.SpeakAsyncCancelAll();
        synthesizer.SpeakAsync(text);
    }

    public void StopSpeech() => synthesizer?.SpeakAsyncCancelAll();

    public void StartRecording(string path)
    {
        if (recording) throw new InvalidOperationException("A recording is already in progress.");
        StopSpeech();
        Send("open new type waveaudio alias academyrecord");
        try
        {
            Send("set academyrecord time format milliseconds");
            Send("record academyrecord");
            recordingPath = path;
            recording = true;
        }
        catch { Send("close academyrecord"); throw; }
    }

    public string StopRecording()
    {
        if (!recording || recordingPath is null) throw new InvalidOperationException("No recording is in progress.");
        try
        {
            Send("stop academyrecord");
            Send($"save academyrecord \"{recordingPath}\"");
            return recordingPath;
        }
        finally
        {
            Send("close academyrecord");
            recording = false;
        }
    }

    public void PlayRecording(string path)
    {
        if (recording) throw new InvalidOperationException("Stop recording before playback.");
        StopPlayback();
        Send($"open \"{path}\" type waveaudio alias academyplay");
        playbackOpen = true;
        Send("play academyplay");
    }

    private bool playbackOpen;
    public void StopPlayback()
    {
        if (!playbackOpen) return;
        Send("close academyplay");
        playbackOpen = false;
    }

    private static void Send(string command)
    {
        var error = MciSendString(command, null, 0, IntPtr.Zero);
        if (error == 0) return;
        var message = new StringBuilder(512);
        MciGetErrorString(error, message, message.Capacity);
        throw new InvalidOperationException($"Windows audio: {message}");
    }

    [DllImport("winmm.dll", EntryPoint = "mciSendStringW", CharSet = CharSet.Unicode)]
    private static extern int MciSendString(string command, StringBuilder? result, int length, IntPtr callback);
    [DllImport("winmm.dll", EntryPoint = "mciGetErrorStringW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MciGetErrorString(int error, StringBuilder message, int length);

    public void Dispose()
    {
        StopSpeech();
        StopPlayback();
        if (recording) StopRecording();
        synthesizer?.Dispose();
    }
}
