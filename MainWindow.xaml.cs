using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TorrentDuplicationChecker.Models;
using TorrentDuplicationChecker.ViewModels;

namespace TorrentDuplicationChecker;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DuplicatesGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm || sender is not DataGrid dg)
            return;

        var selected = new List<DuplicateTorrentRow>(dg.SelectedItems.Count);
        foreach (var o in dg.SelectedItems)
        {
            if (o is DuplicateTorrentRow row)
                selected.Add(row);
        }

        vm.SetSelectedRows(selected);
    }
}
