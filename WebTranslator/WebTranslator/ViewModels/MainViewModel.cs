using ReactiveUI.Fody.Helpers;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class MainViewModel: ViewModelBase
{
    public MainViewModel()
    {
        NavigationService.Register(page =>
        {
            NavigationContent = page switch
            {
                0 => Page1,
                1 => Page2,
                _ => NavigationContent
            };
        });
    }

    private static OpenFileViewModel Page1 = new();
    private static EditorViewModel Page2 = new();

    [Reactive] public ViewModelBase NavigationContent { get; set; } = Page1;
}