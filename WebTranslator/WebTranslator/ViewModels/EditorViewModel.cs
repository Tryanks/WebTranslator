using AvaloniaEdit.Document;
using ReactiveUI.Fody.Helpers;

namespace WebTranslator.ViewModels;

public class EditorViewModel : ViewModelBase
{
    [Reactive] public TextDocument SourceDoc { get; set; } = new();
    [Reactive] public TextDocument TransDoc { get; set; } = new();

    public string SourceText
    {
        get => SourceDoc.Text;
        set => SourceDoc.Text = value;
    }

    public string TransText
    {
        get => TransDoc.Text;
        set => TransDoc.Text = value;
    }
}