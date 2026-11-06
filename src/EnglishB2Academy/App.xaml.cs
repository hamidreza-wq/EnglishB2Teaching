using System.Windows;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace EnglishB2Academy;

public partial class App : Application
{
    private Mutex? instanceMutex;
    private bool ownsMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var index = Array.IndexOf(e.Args, "--data-directory");
            var directory = index >= 0 && index + 1 < e.Args.Length ? Path.GetFullPath(e.Args[index + 1]) :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EnglishB2Academy");
            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(directory.ToUpperInvariant())))[..24];
            instanceMutex = new Mutex(true, $"Local\\EnglishB2Academy-{key}", out ownsMutex);
            if (!ownsMutex)
            {
                MessageBox.Show("English B2 Academy is already running for this learning-data folder. Use the existing window to avoid concurrent editing.",
                    "Already running", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown(0);
                return;
            }
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SqliteException or System.Text.Json.JsonException)
        {
            MessageBox.Show($"English B2 Academy could not open its local data. No reset has been performed.\n\n{ex.Message}",
                "Unable to start", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ownsMutex) instanceMutex?.ReleaseMutex();
        instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
