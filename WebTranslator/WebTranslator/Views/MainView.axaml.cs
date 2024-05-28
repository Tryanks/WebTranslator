using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using WebTranslator.Services;

namespace WebTranslator.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ToastService.Set(TopLevel.GetTopLevel(this));
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem item) return;
        NavigationService.NavigatePage(item.Tag switch
        {
            "OpenFile" => 0,
            "Editor" => 1,
            "ExportFile" => 2,
            _ => 0
        });
    }
}