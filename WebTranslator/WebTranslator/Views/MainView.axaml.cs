using Avalonia;
using Avalonia.Controls;
using WebTranslator.Services;

namespace WebTranslator.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ToastService.Set(TopLevel.GetTopLevel(this));
    }
}