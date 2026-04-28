using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class EditorViewModel : ViewModelBase
{
    public bool IsEmpty { get => field; set => SetProperty(ref field, value); } = true;
    public EditorContentViewModel EditorContent { get => field; set => SetProperty(ref field, value); } = null!;

    public override void SetParameter(object? parameter)
    {
        if (parameter is not ModDictionary modDictionary) return;
        string header;
        if (modDictionary.CurseForgeId == "" && modDictionary.ModNamespace != "")
            header = modDictionary.ModNamespace;
        else if (modDictionary.CurseForgeId != "" && modDictionary.ModNamespace == "")
            header = modDictionary.CurseForgeId;
        else if (modDictionary.CurseForgeId != "" && modDictionary.ModNamespace != "")
            header = $"{modDictionary.CurseForgeId} / {modDictionary.ModNamespace}";
        else
            header = "New Tab";
        var page = new EditorContentViewModel(header, modDictionary);
        IsEmpty = false;
        EditorContent = page;
    }
}
