using AvaloniaEdit.Document;
using ReactiveUI.Fody.Helpers;

namespace WebTranslator.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public TextDocument SourceDoc { get; set; } = new();
    [Reactive] public TextDocument TransDoc { get; set; } = new();
}