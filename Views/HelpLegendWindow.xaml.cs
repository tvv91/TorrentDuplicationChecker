using System.Windows;

namespace TorrentDuplicationChecker.Views;

public partial class HelpLegendWindow
{
    public HelpLegendWindow()
    {
        InitializeComponent();
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
