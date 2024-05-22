using Avalonia;
using Avalonia.Controls;
using WebTranslator.Services;

namespace WebTranslator.Views;

public partial class OpenFileView : UserControl
{
    public OpenFileView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DialogService.RegisterDialog("GithubCommit", TaskDialog.ShowAsync);
    }
}