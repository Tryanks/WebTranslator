using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Services;
using WebTranslator.Views;

namespace WebTranslator.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static readonly OpenFileViewModel Page1 = new();
    private static readonly EditorViewModel Page2 = new();
    private static readonly ExportViewModel Page3 = new();

    private static readonly UserControl NavigationPage1 = new OpenFileView();
    private static readonly UserControl NavigationPage2 = new EditorView();
    private static readonly UserControl NavigationPage3 = new ExportView();

    public MainViewModel()
    {
        NavigationService.Register((page, parameter) =>
        {
            NavigationContent = page switch
            {
                0 => NavigationPage1,
                1 => NavigationPage2,
                2 => NavigationPage3,
                _ => NavigationContent
            };
            NavigationContent.DataContext = page switch
            {
                0 => Page1,
                1 => Page2,
                2 => Page3,
                _ => NavigationContent.DataContext
            };
            if (NavigationContent.DataContext is not ViewModelBase vm) return;
            vm.SetParameter(parameter);
        });
    }

    [Reactive] public UserControl NavigationContent { get; set; } = NavigationPage1;
}