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
    private WindowNotificationManager? NotifyHost { get; set; }
    public OpenFileView()
    {
        InitializeComponent();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (NotifyHost is not null) return;
        NotifyHost = new WindowNotificationManager(TopLevel.GetTopLevel(this))
            { MaxItems = 3, Position = NotificationPosition.BottomRight };
        this.WhenAnyValue(x => x.DataContext)
            .Subscribe(data =>
            {
                var context = data as OpenFileViewModel;
                // NotifyWindow
                context.WhenAnyValue(x => x.NotifyMsg)
                    .Subscribe(msg => NotifyHost?.Show(msg!));
                // TaskDialog
                context.WhenAnyValue(x => x.ShowDialog)
                    .Subscribe(d =>
                    {
                        DialogShow.Title = d?.Title;
                        DialogShow.Content = d?.Content;
                        DialogShow.ShowAsync();
                    });
            });
    }
}