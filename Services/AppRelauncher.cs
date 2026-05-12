using System.Diagnostics;
using System.Windows;

namespace TorrentDuplicationChecker.Services;

public static class AppRelauncher
{
    public static void Restart()
    {
        var path = Environment.ProcessPath;
        if (string.IsNullOrEmpty(path))
        {
            Application.Current.Shutdown();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });

        Application.Current.Shutdown();
    }
}
