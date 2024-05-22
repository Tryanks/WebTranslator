using Avalonia.Controls;
using Avalonia.Interactivity;
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
}