using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnglishB2Academy;

public sealed record Preferences
{
    public string Theme { get; init; } = "Light";
    public int DailyMinutes { get; init; } = 20;
    public string TextSize { get; init; } = "Standard";
}

[JsonSerializable(typeof(Preferences))]
internal partial class PreferencesJsonContext : JsonSerializerContext;

public sealed class PreferencesStore
{
    public static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EnglishB2Academy", "preferences.json");

    private readonly string filePath;

    public PreferencesStore(string? filePath = null) => this.filePath = filePath ?? DefaultFilePath;

    public Preferences Load()
    {
        if (!File.Exists(filePath)) return new();
        var value = JsonSerializer.Deserialize(File.ReadAllText(filePath), PreferencesJsonContext.Default.Preferences)
            ?? throw new InvalidDataException("The preferences file is empty.");
        if (value.Theme is not ("Light" or "Dark") ||
            value.DailyMinutes is not (10 or 20 or 30 or 45) ||
            value.TextSize is not ("Standard" or "Large"))
            throw new InvalidDataException("The preferences file contains unsupported settings.");
        return value;
    }

    public void Save(Preferences value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var temporary = filePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(value, PreferencesJsonContext.Default.Preferences));
        File.Move(temporary, filePath, overwrite: true);
    }
}
