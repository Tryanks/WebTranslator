using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class EditorViewModel : ViewModelBase
{
    [Reactive] public ObservableCollection<EditorPageModel> EditorPages { get; set; } = new();
    [Reactive] public EditorPageModel? SelectedEditorPage { get; set; }

    public void AppendReader(JsonReader reader)
    {
        var content = new EditorContent();
        reader.ElementList.ForEach(e =>
        {
            content.SourceText += e.EnValue + "\n";
            content.TransText += e.ZhValue + "\n";
        });
        string header;
        if (reader.CfID == "" && reader.ModID != "")
            header = reader.ModID;
        else if (reader.CfID != "" && reader.ModID == "")
            header = reader.CfID;
        else if (reader.CfID != "" && reader.ModID != "")
            header = $"{reader.CfID} / {reader.ModID}";
        else
            header = "New Tab";
        var page = new EditorPageModel
        {
            Header = header,
            EditorContent = content
        };
        EditorPages.Add(page);
        SelectedEditorPage = page;
    }
}

public class EditorPageModel : ViewModelBase
{
    [Reactive] public string Header { get; set; } = "New Tab";
    [Reactive] public EditorContent EditorContent { get; set; } = new();
}

public class EditorContent : ViewModelBase
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