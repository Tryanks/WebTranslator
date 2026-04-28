using Avalonia.Controls;
using Avalonia.Controls.Templates;
using WebTranslator.ViewModels;
using WebTranslator.Views;

namespace WebTranslator;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        return data switch
        {
            MainViewModel => new MainView(),
            OpenFileViewModel => new OpenFileView(),
            EditorViewModel => new EditorView(),
            ExportViewModel => new ExportView(),
            EditorContentViewModel => new EditorContentView(),
            _ => new TextBlock { Text = "Not Found: " + data.GetType().Name }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
