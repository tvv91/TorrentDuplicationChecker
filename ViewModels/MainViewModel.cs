using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MessageBox = System.Windows.MessageBox;
using TorrentDuplicationChecker.Localization;
using TorrentDuplicationChecker.Models;
using TorrentDuplicationChecker.Services;
using TorrentDuplicationChecker.Views;

namespace TorrentDuplicationChecker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly List<DuplicateTorrentRow> _selectedRows = new();
    private bool _suppressClientKindPersistence;

    public MainViewModel(AppSettings settings)
    {
        _settings = settings;
        _suppressClientKindPersistence = true;
        SelectedClientKindId = settings.SelectedTorrentClientKindId;
        _suppressClientKindPersistence = false;
        ClientOptions = [.. TorrentClientRegistry.All];
        StatusText = Strings.Status_Ready;

        var rowsView = CollectionViewSource.GetDefaultView(Rows);
        rowsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DuplicateTorrentRow.DuplicateClusterId)));
    }

    public IReadOnlyList<TorrentClientOption> ClientOptions { get; }

    public ObservableCollection<DuplicateTorrentRow> Rows { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public bool IsIdle => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsIdle));
        CancelAnalyzeCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _statusText = "";

    [ObservableProperty]
    private string _selectedClientKindId = "";

    /// <summary>Called from the view when DataGrid selection changes.</summary>
    public void SetSelectedRows(IReadOnlyList<DuplicateTorrentRow> items)
    {
        _selectedRows.Clear();
        if (items.Count > 0)
            _selectedRows.AddRange(items);
        OpenSaveFolderCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedClientKindIdChanged(string value)
    {
        if (_suppressClientKindPersistence)
            return;

        _settings.SelectedTorrentClientKindId = value;
        AppSettingsStore.Save(_settings);
    }

    [RelayCommand]
    private void OpenHelpLegend()
    {
        var w = new HelpLegendWindow
        {
            Owner = Application.Current.MainWindow
        };
        w.ShowDialog();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var previousUiLanguage = _settings.UiLanguage;
        var dialogVm = new SettingsViewModel(_settings.Clone());
        var w = new SettingsWindow
        {
            DataContext = dialogVm,
            Owner = Application.Current.MainWindow
        };
        if (w.ShowDialog() == true)
        {
            _settings.CopyFrom(dialogVm.Settings);
            AppSettingsStore.Save(_settings);
            _suppressClientKindPersistence = true;
            try
            {
                SelectedClientKindId = _settings.SelectedTorrentClientKindId;
            }
            finally
            {
                _suppressClientKindPersistence = false;
            }

            if (!string.Equals(previousUiLanguage, _settings.UiLanguage, StringComparison.OrdinalIgnoreCase))
            {
                AppRelauncher.Restart();
                return;
            }

            StatusText = Strings.Msg_SettingsSaved;
        }
    }

    [RelayCommand]
    private async Task AnalyzeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebUiBaseUrl))
        {
            MessageBox.Show(Strings.Msg_SpecifyWebUiUrl,
                Strings.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dispatcher = Application.Current!.Dispatcher;

        IsBusy = true;
        StatusText = Strings.Analyze_StatusConnecting;
        Rows.Clear();
        SetSelectedRows([]);

        try
        {
            await using var client = TorrentClientRegistry.Create(_settings);

            await dispatcher
                .InvokeAsync(() => StatusText = Strings.Analyze_StatusLoadingTorrents, DispatcherPriority.Normal,
                    cancellationToken)
                .Task.ConfigureAwait(false);

            var torrents = await client.GetTorrentsAsync(cancellationToken).ConfigureAwait(false);

            await dispatcher
                .InvokeAsync(() => StatusText = Strings.Analyze_StatusAnalyzing, DispatcherPriority.Normal,
                    cancellationToken)
                .Task.ConfigureAwait(false);

            var found = await Task.Run(() => DuplicateTorrentAnalyzer.Analyze(torrents), cancellationToken)
                .ConfigureAwait(false);

            await dispatcher
                .InvokeAsync(
                    () =>
                    {
                        foreach (var row in found)
                            Rows.Add(row);

                        StatusText = found.Count == 0
                            ? Strings.Analyze_StatusNoDuplicates
                            : Strings.Analyze_StatusComplete_Format(found.Count);
                    },
                    DispatcherPriority.Normal,
                    cancellationToken)
                .Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await dispatcher
                .InvokeAsync(
                    () => StatusText = Strings.Analyze_StatusCancelled,
                    DispatcherPriority.Normal,
                    CancellationToken.None)
                .Task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await dispatcher
                .InvokeAsync(
                    () =>
                    {
                        StatusText = Strings.Analyze_StatusError;
                        MessageBox.Show(ex.Message, Strings.AppTitle, MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    },
                    DispatcherPriority.Normal,
                    CancellationToken.None)
                .Task.ConfigureAwait(false);
        }
        finally
        {
            await dispatcher
                .InvokeAsync(() => IsBusy = false, DispatcherPriority.Normal, CancellationToken.None)
                .Task.ConfigureAwait(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelAnalyze))]
    private void CancelAnalyze()
    {
        if (AnalyzeCommand is IAsyncRelayCommand asyncCmd)
            asyncCmd.Cancel();
    }

    private bool CanCancelAnalyze() => IsBusy;

    [RelayCommand(CanExecute = nameof(CanOpenSaveFolder))]
    private void OpenSaveFolder()
    {
        if (_selectedRows.Count == 0)
            return;

        foreach (var row in _selectedRows)
        {
            var path = row.SavePath.Trim();
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                continue;

            try
            {
                var normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{normalized}\"",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = normalized,
                        UseShellExecute = true
                    });
                }

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Strings.Msg_OpenFolderFailedTitle, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

        MessageBox.Show(Strings.Msg_OpenFolderInvalidPath, Strings.AppTitle, MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private bool CanOpenSaveFolder() => _selectedRows.Count > 0;
}
