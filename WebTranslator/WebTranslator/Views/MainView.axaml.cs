using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using WebTranslator.ViewModels;

namespace WebTranslator.Views;

public partial class MainView : UserControl
{
    public OpenFileView OpenFilePage { get; set; } = new();
    public EditorView EditorPage { get; set; } = new();
    public ExportFileView ExportFilePage { get; set; } = new();
    
    public OpenFileViewModel OpenFileViewModel { get; set; } = new();
    public EditorViewModel EditorViewModel { get; set; } = new();
    public ExportFileViewModel ExportFileViewModel { get; set; } = new();
    
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OpenFilePage.DataContext = OpenFileViewModel;
        EditorPage.DataContext = EditorViewModel;
        ExportFilePage.DataContext = ExportFileViewModel;
        OpenFileViewModel.WhenAnyValue(x => x.OutJson)
            .Subscribe(reader =>
            {
                if (reader is null) return;
                EditorViewModel.AppendReader(reader);
                NavView.SelectedItem = NavView.MenuItems[1];
            });
    }

    private void NavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem item) return;
        NavView.Content = item.Tag switch
        {
            "OpenFile" => OpenFilePage,
            "Editor" => EditorPage,
            "ExportFile" => ExportFilePage,
            _ => NavView.Content
        };
    }
}