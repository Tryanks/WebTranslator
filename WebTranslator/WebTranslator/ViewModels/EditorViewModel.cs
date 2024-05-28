using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class EditorViewModel : ViewModelBase
{
    public EditorViewModel()
    {
        EditorPages.WhenAnyValue(x => x.Count)
            .Subscribe(x => IsEmpty = x == 0);
    }
    [Reactive] public bool IsEmpty { get; set; } = true;
    [Reactive] public ObservableCollection<EditorPageModel> EditorPages { get; set; } = new();
    [Reactive] public EditorPageModel? SelectedEditorPage { get; set; }

    // public void AppendReader(JsonReader reader)
    // {
    //     string header;
    //     if (reader.CfID == "" && reader.ModID != "")
    //         header = reader.ModID;
    //     else if (reader.CfID != "" && reader.ModID == "")
    //         header = reader.CfID;
    //     else if (reader.CfID != "" && reader.ModID != "")
    //         header = $"{reader.CfID} / {reader.ModID}";
    //     else
    //         header = "New Tab";
    //     var page = new EditorPageModel
    //     {
    //         Header = header,
    //     };
    //     reader.ElementList.ForEach(e => { page.AddItem(e.EnValue, e.ZhValue); });
    //
    //     EditorPages.Add(page);
    //     SelectedEditorPage = page;
    // }
}

public class EditorPageModel : ViewModelBase
{
    public EditorPageModel()
    {
        this.WhenAnyValue(x => x.SelectedIndex).Subscribe(x =>
        {
            if (x == -1) return;
            var editor = EditorList[x];
            SourceText = editor.EnText;
            TransText = editor.ZhText;
        });
    }
    [Reactive] public string Header { get; set; } = "New Tab";
    
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
    
    public void UpdateItem()
    {
        if (SelectedIndex <= 0 || SelectedIndex >= EditorList.Count - 1) return;
        var editor = EditorList[SelectedIndex];
        editor.ZhText = TransText;
        SelectNextItem();
    }
    
    public bool SelectNextItem()
    {
        if (SelectedIndex == -1) return false;
        if (SelectedIndex == EditorList.Count - 1) return false;
        SelectedIndex++;
        return true;
    }
    
    public bool SelectPrevItem()
    {
        if (SelectedIndex <= 0) return false;
        SelectedIndex--;
        return true;
    }
}

public class EditorListItem : ViewModelBase
{
    [Reactive] public string EnText { get; set; }
    [Reactive] public string ZhText { get; set; }
    [Reactive] public bool IsTranslated { get; set; }

    public EditorListItem(string en, string zh)
    {
        EnText = en;
        this.WhenAnyValue(x => x.ZhText)
            .Subscribe(s => IsTranslated = s != EnText && !string.IsNullOrEmpty(s));
        ZhText = zh;
    }
}