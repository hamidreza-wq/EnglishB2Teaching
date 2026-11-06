using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace EnglishB2Academy.Core;

public sealed class LearningStore : IDisposable
{
    private readonly SqliteConnection connection;
    public string FilePath { get; }
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public LearningStore(string path)
    {
        FilePath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = FilePath, Pooling = false
        }.ToString());
        try
        {
            connection.Open();
            using var version = connection.CreateCommand();
            version.CommandText = "PRAGMA user_version";
            var schema = Convert.ToInt32(version.ExecuteScalar());
            if (schema > 1) throw new InvalidDataException("This database was created by a newer application. Update the app before opening it.");
            Execute("PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;");
            using var transaction = connection.BeginTransaction();
            using var migration = connection.CreateCommand();
            migration.Transaction = transaction;
            migration.CommandText = """
                CREATE TABLE IF NOT EXISTS sessions(id TEXT PRIMARY KEY, payload TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS drafts(id TEXT PRIMARY KEY, payload TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS settings(id TEXT PRIMARY KEY, payload TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS cards(id TEXT PRIMARY KEY, payload TEXT NOT NULL);
                PRAGMA user_version=1;
                """;
            migration.ExecuteNonQuery();
            transaction.Commit();
            Sessions();
            Drafts();
            Profile();
            ReviewCards();
        }
        catch { connection.Dispose(); throw; }
    }

    private void Execute(string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private void Save<T>(string table, string id, T value)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {table}(id,payload) VALUES ($id,$payload) ON CONFLICT(id) DO UPDATE SET payload=excluded.payload";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$payload", JsonSerializer.Serialize(value, JsonOptions));
        command.ExecuteNonQuery();
    }

    private List<T> Read<T>(string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT payload FROM {table} ORDER BY id";
        using var reader = command.ExecuteReader();
        var result = new List<T>();
        while (reader.Read())
            result.Add(JsonSerializer.Deserialize<T>(reader.GetString(0))
                ?? throw new InvalidDataException($"A record in {table} is empty."));
        return result;
    }

    public void SaveSession(PracticeSession session)
    {
        ValidateSession(session);
        Save("sessions", session.Id, session);
    }
    public List<PracticeSession> Sessions()
    {
        var result = Read<PracticeSession>("sessions");
        foreach (var session in result) ValidateSession(session);
        return result.OrderByDescending(s => s.Started).ToList();
    }
    public void SaveDraft(Draft draft)
    {
        ValidateDraft(draft);
        Save("drafts", draft.Id, draft);
    }
    public List<Draft> Drafts()
    {
        var drafts = Read<Draft>("drafts");
        foreach (var draft in drafts) ValidateDraft(draft);
        return drafts.OrderByDescending(d => d.Updated).ToList();
    }
    public LearnerProfile Profile()
    {
        var profiles = Read<LearnerProfile>("settings");
        if (profiles.Count > 1) throw new InvalidDataException("Multiple learner profiles were found.");
        var profile = profiles.SingleOrDefault() ?? new();
        ValidateProfile(profile);
        return profile;
    }
    public void SaveProfile(LearnerProfile profile)
    {
        ValidateProfile(profile);
        Save("settings", "profile", profile);
    }
    public List<ReviewCard> ReviewCards()
    {
        var cards = Read<ReviewCard>("cards");
        foreach (var card in cards) ValidateCard(card);
        return cards;
    }

    public void Review(string word, bool recalled, DateTimeOffset now)
    {
        var previous = ReviewCards().SingleOrDefault(c => c.Word == word);
        var days = recalled ? Math.Min(60, Math.Max(1, (previous?.IntervalDays ?? 0) * 2)) : 0;
        Save("cards", word, new ReviewCard(word, days, recalled ? now.AddDays(days) : now.AddMinutes(10)));
    }

    public void Backup(string destination)
    {
        var target = Path.GetFullPath(destination);
        if (string.Equals(target, FilePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Choose a backup file different from the live database.");
        if (File.Exists(target)) throw new IOException("The backup destination already exists. Choose a new filename.");
        using var backup = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = target, Pooling = false }.ToString());
        backup.Open();
        connection.BackupDatabase(backup);
    }

    public static void ValidateBackup(string source)
    {
        if (!File.Exists(source)) throw new FileNotFoundException("Backup does not exist.", source);
        if (new FileInfo(source).Length > 200 * 1024 * 1024)
            throw new InvalidDataException("This backup exceeds the 200 MB import limit.");
        using var candidate = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(source), Mode = SqliteOpenMode.ReadOnly, Pooling = false
        }.ToString());
        candidate.Open();
        using var command = candidate.CreateCommand();
        command.CommandText = "PRAGMA quick_check";
        if (!string.Equals(command.ExecuteScalar()?.ToString(), "ok", StringComparison.Ordinal))
            throw new InvalidDataException("Backup integrity check failed.");
        command.CommandText = "PRAGMA user_version";
        if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new InvalidDataException("Unsupported backup schema.");
        foreach (var table in new[] { "sessions", "drafts", "settings", "cards" })
        {
            command.CommandText = $"SELECT id,payload FROM {table}";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var json = reader.GetString(1);
                var id = reader.GetString(0);
                switch (table)
                {
                    case "sessions":
                        var session = JsonSerializer.Deserialize<PracticeSession>(json) ?? throw new InvalidDataException("Empty session.");
                        ValidateSession(session);
                        if (session.Id != id) throw new InvalidDataException("Session identifier mismatch.");
                        break;
                    case "drafts":
                        var draft = JsonSerializer.Deserialize<Draft>(json) ?? throw new InvalidDataException("Empty draft.");
                        ValidateDraft(draft);
                        if (draft.Id != id) throw new InvalidDataException("Draft identifier mismatch.");
                        break;
                    case "settings":
                        var profile = JsonSerializer.Deserialize<LearnerProfile>(json) ?? throw new InvalidDataException("Empty profile.");
                        ValidateProfile(profile);
                        if (id != "profile") throw new InvalidDataException("Unknown profile key.");
                        break;
                    case "cards":
                        var card = JsonSerializer.Deserialize<ReviewCard>(json) ?? throw new InvalidDataException("Empty vocabulary card.");
                        ValidateCard(card);
                        if (card.Word != id) throw new InvalidDataException("Card identifier mismatch.");
                        break;
                }
            }
        }
    }

    public void Restore(string source)
    {
        if (string.Equals(Path.GetFullPath(source), FilePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Select an external backup, not the live database.");
        ValidateBackup(source);
        using var candidate = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(source), Mode = SqliteOpenMode.ReadOnly, Pooling = false
        }.ToString());
        candidate.Open();
        candidate.BackupDatabase(connection);
    }

    public string ExportJson() => JsonSerializer.Serialize(new
    {
        Schema = 1, Exported = DateTimeOffset.UtcNow, Profile = Profile(),
        Sessions = Sessions(), Drafts = Drafts(), Cards = ReviewCards()
    }, JsonOptions);

    public void DeleteLearningRecords()
    {
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM sessions; DELETE FROM drafts; DELETE FROM settings; DELETE FROM cards;";
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    private static void ValidateSession(PracticeSession session)
    {
        if (string.IsNullOrWhiteSpace(session.Id) || string.IsNullOrWhiteSpace(session.Title) ||
            session.Questions is null || session.Responses is null || session.AudioPlays is null ||
            session.Questions.Count is < 1 or > 1000 || session.Position < 0 || session.Position >= session.Questions.Count ||
            session.ActiveSeconds < 0 || session.Deadline < session.Started || session.Finished < session.Started ||
            session.Mode is not ("Lesson" or "Diagnostic" or "Exam" or "Revision"))
            throw new InvalidDataException("A saved session contains invalid state.");
        if (session.Questions.Any(q => q is null || string.IsNullOrWhiteSpace(q.Id) ||
                q.Prompt is null || q.Options is null || q.Answers is null || q.Answers.Length == 0 ||
                q.Answers.Any(string.IsNullOrWhiteSpace) || q.Explanation is null || q.Passage is null || q.Audio is null) ||
            session.Questions.Select(q => q.Id).Distinct().Count() != session.Questions.Count ||
            session.Responses.Any(p => p.Value is null || !session.Questions.Any(q => q.Id == p.Key)) ||
            session.AudioPlays.Any(p => p.Value < 0))
            throw new InvalidDataException("A saved session contains invalid questions or responses.");
    }

    private static void ValidateDraft(Draft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Id) || draft.Kind is not ("Writing" or "Timed writing") ||
            draft.Text is null || draft.Text.Length > 20000 || string.IsNullOrWhiteSpace(draft.Prompt))
            throw new InvalidDataException("A saved draft is invalid.");
        if (draft.Kind == "Timed writing" && !DateTimeOffset.TryParse(
            draft.Prompt.Split('\n')[0].Replace("Deadline (UTC): ", ""),
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out _))
            throw new InvalidDataException("A timed draft has an invalid deadline.");
    }

    private static void ValidateProfile(LearnerProfile profile)
    {
        if (profile.KnownLevel is not ("Not sure" or "A1" or "A2" or "B1" or "B1+" or "B2" or "C1" or "C2") ||
            profile.Variety is not ("British English" or "American English") ||
            profile.Pace is not ("Gentle" or "Balanced" or "Intensive") ||
            profile.FirstLanguage is null || profile.FirstLanguage.Length > 100 || profile.ExamDate is null ||
            (profile.ExamDate.Length > 0 && !DateOnly.TryParseExact(profile.ExamDate, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _)))
            throw new InvalidDataException("A saved learner profile is invalid.");
    }

    private static void ValidateCard(ReviewCard card)
    {
        if (string.IsNullOrWhiteSpace(card.Word) || card.IntervalDays is < 0 or > 60)
            throw new InvalidDataException("A vocabulary review schedule is invalid.");
    }

    public void Dispose() => connection.Dispose();
}
