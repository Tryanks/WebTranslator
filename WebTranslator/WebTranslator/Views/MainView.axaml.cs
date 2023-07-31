using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using FluentAvalonia.UI.Controls;
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
    }

    private void NavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem item) return;
        switch (item.Tag)
        {
            case "OpenFile":
                NavView.Content = OpenFilePage;
                break;
            case "Editor":
                NavView.Content = EditorPage;
                break;
            case "ExportFile":
                NavView.Content = ExportFilePage;
                break;
        }
    }
}