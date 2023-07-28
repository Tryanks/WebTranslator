using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ReactiveUI;
using WebTranslator.ViewModels;

namespace WebTranslator.Views;

public partial class OpenFileView : UserControl
{
    public WindowNotificationManager? NotifyHost { get; set; }
    public OpenFileView()
    {
        InitializeComponent();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        NotifyHost = new WindowNotificationManager(topLevel)
            { MaxItems = 3, Position = NotificationPosition.BottomRight };
        this.WhenAnyValue(x => x.DataContext)
            .Subscribe(data =>
            {
                var context = data as OpenFileViewModel;
                // NotifyWindow
                context.WhenAnyValue(x => x.notifyMsg)
                    .Subscribe(msg => NotifyHost?.Show(msg!));
                // TaskDialog
                context.WhenAnyValue(x => x.showDialog)
                    .Subscribe(d =>
                    {
                        DialogShow.Title = d?.Title;
                        DialogShow.Content = d?.Content;
                        DialogShow.ShowAsync();
                    });
            });
    }
    
    // private void Button_OnClickZh(object? sender, RoutedEventArgs e)
    // {
    //     var data = DataContext as OpenFileViewModel;
    //     DialogShow.Content = data?.ZhText;
    //     DialogShow.Title = "zh_cn.json";
    //     DialogShow.ShowAsync();
    // }
    //
    // private void Button_OnClickEn(object? sender, RoutedEventArgs e)
    // {
    //     var data = DataContext as OpenFileViewModel;
    //     DialogShow.Content = data?.EnText;
    //     DialogShow.Title = "en_us.json";
    //     DialogShow.ShowAsync();
    // }
}