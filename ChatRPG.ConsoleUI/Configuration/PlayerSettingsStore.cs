using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatRPG.ConsoleUI.Configuration;

public sealed class PlayerSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string DirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChatRPG");

    public string FilePath => Path.Combine(DirectoryPath, "settings.json");

    public string HistoryPath => Path.Combine(DirectoryPath, "history.txt");

    public PlayerSettings Load()
    {
        PlayerSettings settings;
        try
        {
            settings = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<PlayerSettings>(File.ReadAllText(FilePath), SerializerOptions) ?? new PlayerSettings()
                : new PlayerSettings();
        }
        catch (JsonException)
        {
            settings = new PlayerSettings();
        }

        if (settings.IdentityId == Guid.Empty)
        {
            settings = settings with { IdentityId = Guid.NewGuid() };
            Save(settings);
        }

        return settings;
    }

    public void Save(PlayerSettings settings)
    {
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, SerializerOptions));
    }
}
