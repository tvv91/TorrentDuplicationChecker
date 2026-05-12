namespace TorrentDuplicationChecker.Localization;

public sealed record UiLanguageOption(string Code, string DisplayName);

public static class UiLanguages
{
    public static IReadOnlyList<UiLanguageOption> All { get; } =
    [
        new("en", "English"),
        new("uk", "Українська"),
        new("ru", "Русский"),
    ];
}
