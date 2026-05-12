namespace TorrentDuplicationChecker.Models;

public sealed class AppSettings
{
    public string SelectedTorrentClientKindId { get; set; } = "qbittorrent";

    /// <summary>UI culture: en, uk, ru.</summary>
    public string UiLanguage { get; set; } = "en";

    public string WebUiBaseUrl { get; set; } = "http://localhost:8080";

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public AppSettings Clone()
    {
        return new AppSettings
        {
            SelectedTorrentClientKindId = SelectedTorrentClientKindId,
            UiLanguage = UiLanguage,
            WebUiBaseUrl = WebUiBaseUrl,
            UserName = UserName,
            Password = Password
        };
    }

    public void CopyFrom(AppSettings other)
    {
        SelectedTorrentClientKindId = other.SelectedTorrentClientKindId;
        UiLanguage = other.UiLanguage;
        WebUiBaseUrl = other.WebUiBaseUrl;
        UserName = other.UserName;
        Password = other.Password;
    }
}
