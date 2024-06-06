using AvaloniaEdit.Document;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class ExportViewModel : ViewModelBase
{
    public TextDocument Document { get; set; } = new();

    public override void SetParameter(object? parameter)
    {
        if (parameter is not ModDictionary dict) return;
        Document.Text = dict.ToString();
    }
}