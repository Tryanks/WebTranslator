using Avalonia.Controls;
using Avalonia.Interactivity;
using WebTranslator.ViewModels;

namespace WebTranslator.Views;

public partial class OpenFileView : UserControl
{
    public OpenFileView()
    {
        InitializeComponent();
    }
    
    private void Button_OnClickZh(object? sender, RoutedEventArgs e)
    {
        var data = (OpenFileViewModel)DataContext!;
        DialogShow.Content = data.ZhText;
        DialogShow.ShowAsync();
    }
    
    private void Button_OnClickEn(object? sender, RoutedEventArgs e)
    {
        var data = (OpenFileViewModel)DataContext!;
        DialogShow.Content = data.EnText;
        DialogShow.ShowAsync();
    }
}