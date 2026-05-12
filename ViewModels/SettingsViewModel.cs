using CommunityToolkit.Mvvm.ComponentModel;
using TorrentDuplicationChecker.Localization;
using TorrentDuplicationChecker.Models;
using TorrentDuplicationChecker.Services;

namespace TorrentDuplicationChecker.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(AppSettings settings)
    {
        Settings = settings;
        ClientOptions = [.. TorrentClientRegistry.All];
        _selectedClientKindId = settings.SelectedTorrentClientKindId;
    }

    public AppSettings Settings { get; }

    public IReadOnlyList<TorrentClientOption> ClientOptions { get; }

    public IReadOnlyList<UiLanguageOption> LanguageOptions => UiLanguages.All;

    [ObservableProperty]
    private string _selectedClientKindId = "";

    partial void OnSelectedClientKindIdChanged(string value)
    {
        Settings.SelectedTorrentClientKindId = value;
    }
}
