using System.Windows;
using System.Windows.Controls;
using TorrentDuplicationChecker.Models;
using TorrentDuplicationChecker.ViewModels;

namespace TorrentDuplicationChecker.Views;

public partial class SettingsWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            PasswordBox.Password = vm.Settings.Password ?? "";
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.Settings.Password = PasswordBox.Password;
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.Settings.Password = PasswordBox.Password;

        DialogResult = true;
    }
}
