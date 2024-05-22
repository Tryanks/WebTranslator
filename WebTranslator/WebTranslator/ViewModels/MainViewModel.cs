using ReactiveUI.Fody.Helpers;

namespace WebTranslator.ViewModels;

public class MainViewModel
{
    [Reactive] public uint NavigationTabIdx { get; set; }
    [Reactive] public ViewModelBase NavigationContent { get; set; } = new OpenFileViewModel();
}