using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ReactiveUI;
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
            content.AddItem(e.EnValue, e.ZhValue);
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
    [Reactive] public ObservableCollection<EditorListItem> EditorList { get; set; } = new();
    [Reactive] public int SelectedIndex { get; set; } = -1;
    public void AddItem(string en, string zh)
    {
        EditorList.Add(new EditorListItem(en, zh));
        if (SelectedIndex == -1)
            SelectedIndex = 0;
    }
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

public class EditorListItem : ViewModelBase
{
    [Reactive] public string EnText { get; set; }
    [Reactive] public string ZhText { get; set; }
    [Reactive] public bool IsTranslated { get; set; } = false;

    public EditorListItem(string en, string zh)
    {
        EnText = en;
        this.WhenAnyValue(x => x.ZhText).Subscribe(s =>
        {
            if (s != "" && s != EnText)
                IsTranslated = true;
            else
                IsTranslated = false;
        });
        ZhText = zh;
    }
}