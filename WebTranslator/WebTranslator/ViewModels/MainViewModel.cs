using ReactiveUI.Fody.Helpers;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static readonly OpenFileViewModel Page1 = new();
    private static readonly EditorViewModel Page2 = new();
    private static readonly ExportViewModel Page3 = new();

    public MainViewModel()
    {
        NavigationService.Register((page, parameter) =>
        {
            NavigationContent = page switch
            {
                0 => Page1,
                1 => Page2,
                2 => Page3,
                _ => NavigationContent
            };
            NavigationContent.SetParameter(parameter);
        });
    }

    [Reactive] public ViewModelBase NavigationContent { get; set; } = Page1;
}