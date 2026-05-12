using System.Windows;
using TorrentDuplicationChecker.Localization;
using TorrentDuplicationChecker.Services;
using TorrentDuplicationChecker.ViewModels;

namespace TorrentDuplicationChecker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var settings = AppSettingsStore.Load();
        AppCulture.Apply(settings.UiLanguage);
        var main = new MainWindow
        {
            DataContext = new MainViewModel(settings)
        };
        MainWindow = main;
        main.Show();
    }
}
