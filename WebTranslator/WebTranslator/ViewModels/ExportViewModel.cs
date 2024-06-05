using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class ExportViewModel : ViewModelBase
{
    [Reactive] public string Greeting { get; set; } = "Hello, WebTranslator Exporter!";

    public override void SetParameter(object? parameter)
    {
        if (parameter is not ModDictionary dict) return;
        ToastService.Notify("Not Implemented Yet");
        Greeting = dict.ToString();
    }
}