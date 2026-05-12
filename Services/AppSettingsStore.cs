using System.Text.Json;
using TorrentDuplicationChecker.Models;

namespace TorrentDuplicationChecker.Services;

public static class AppSettingsStore
{
    private static readonly HashSet<string> SupportedUiLanguages = new(["en", "uk", "ru"], StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TorrentDuplicationChecker", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            var path = FilePath;
            if (!File.Exists(path))
                return DefaultSettings();

            var json = File.ReadAllText(path);
            var s = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (s is null)
                return DefaultSettings();

            NormalizeUiLanguage(s);
            return s;
        }
        catch
        {
            return DefaultSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    private static AppSettings DefaultSettings()
    {
        var s = new AppSettings();
        NormalizeUiLanguage(s);
        return s;
    }

    private static void NormalizeUiLanguage(AppSettings settings)
    {
        var code = (settings.UiLanguage ?? "").Trim();
        settings.UiLanguage = SupportedUiLanguages.Contains(code) ? code.ToLowerInvariant() : "en";
    }
}
