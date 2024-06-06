using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Controls;
using WebTranslator.Interfaces;
using WebTranslator.Models;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class OpenFileViewModel : ViewModelBase
{
    public OpenFileViewModel()
    {
        ExtensionMethods.Subscribe(this.WhenAnyValue(x => x.GithubLink), link => { GithubLinkStatus.CheckLink(link); });
    }

    [Reactive] public uint TabSelectedIndex { get; set; }
    [Reactive]
    public string GithubLink { get; set; } =
#if DEBUG
        "https://github.com/CFPAOrg/Minecraft-Mod-Language-Package/tree/cf1a801466a901ec14a28c3b08dfe62f69c8db53/projects/1.20/assets/adorn-for-forge/adorn/lang";
#else
        "";
#endif
    [Reactive] internal GithubLinkStatus GithubLinkStatus { get; set; } = new();
    [Reactive] internal LanguageChoice LanguageChoice { get; set; } = new();
    [Reactive] public bool EnableDocument { get; set; }
    [Reactive] public TextDocument OriginalDocument { get; set; } = new();
    [Reactive] public TextDocument TranslatedDocument { get; set; } = new();

    private string OriginalText
    {
        get => OriginalDocument.Text;
        set => OriginalDocument.Replace(0, OriginalDocument.TextLength, value);
    }

    private string TranslatedText
    {
        get => TranslatedDocument.Text;
        set => TranslatedDocument.Replace(0, TranslatedDocument.TextLength, value);
    }

    public async void GithubCommand()
    {
        if (string.IsNullOrEmpty(GithubLink))
        {
            ToastService.Notify("打开失败", "Github 链接为空", NotificationType.Warning);
            return;
        }

        if (!GithubLinkStatus.Passed())
        {
            ToastService.Notify("打开失败", "输入的链接不满足条件", NotificationType.Warning);
            return;
        }

        var link = GithubHelper.GithubConvert(GithubLink);
        LanguageChoice.IsLoading = false;
        LanguageChoice.Success = false;

        var dialog = new ContentDialog
        {
            Title = "Load Github Files",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            Content = new ConfirmDialog
            {
                DataContext = LanguageChoice
            }
        };

        var confirm = false;

        dialog.Opened += DialogLoaded;
        dialog.PrimaryButtonClick += DialogConfirmed;
        dialog.Closed += DialogClosed;
        LanguageChoice.OnLoaded += DialogDownloaded;

        await dialog.ShowAsync();
        return;

        async void DialogLoaded(object? sender, object? e)
        {
            dialog.Opened -= DialogLoaded;

            LanguageChoice.IsLoading = true;

            ToastService.Notify("正在获取文件列表，请稍后");
            var infos = await GithubHelper.GetLanguageFilesAsync(link);
            if (infos.Count == 0)
            {
                ToastService.Notify("获取到的文件列表为空，请确定存在语言文件", NotificationType.Error);
                LanguageChoice.IsLoading = false;
                return;
            }

            ToastService.Notify("获取文件列表成功");

            dialog.IsPrimaryButtonEnabled = false;
            LanguageChoice.SetFileInfos(infos);
            LanguageChoice.IsLoading = false;
            LanguageChoice.Success = true;
        }

        void DialogConfirmed(object? sender, object? e)
        {
            dialog.PrimaryButtonClick -= DialogConfirmed;
            confirm = true;
        }

        void DialogClosed(object? sender, object? e)
        {
            dialog.Closed -= DialogClosed;
            if (!confirm) return;

            GithubConfirmCommand();
        }

        void DialogDownloaded()
        {
            LanguageChoice.OnLoaded -= DialogDownloaded;
            dialog.IsPrimaryButtonEnabled = true;
        }
    }

    private async void GithubConfirmCommand()
    {
        if (!LanguageChoice.Success)
        {
            ToastService.Notify("请先获取文件列表", NotificationType.Error);
            return;
        }

        if (LanguageChoice.SelectOriginal is null)
        {
            ToastService.Notify("请选择原文文件", NotificationType.Error);
            return;
        }

        OriginalText = await LanguageChoice.SelectOriginal!.String();
        if (LanguageChoice.SelectTranslated is null)
            TranslatedText = "";
        else
            TranslatedText = await LanguageChoice.SelectTranslated!.String();

        TabSelectedIndex = 1;
        _previewFileStatus = ImportFileMode.Github;
    }

    public async void OpenFileCommand(string s)
    {
        switch (s)
        {
            case "Folder":
                await OpenFolder();
                break;
            case "Original":
                await OpenOriginalFile();
                break;
            case "Translated":
                await OpenTranslatedFile();
                break;
            default:
                ToastService.Notify("无法打开文件", $"未知文件类型: {s}", NotificationType.Error);
                break;
        }
    }

    private IStorageFolder? Folder { get; set; }

    private async Task OpenFolder()
    {
        var folders = await FilePickerService.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (folders.Count != 1) return;
        Folder = folders[0];
        var files = Folder.GetItemsAsync();
        var fileList = new List<IFileInfo>();
        await foreach (var file in files)
            if (file.Name.EndsWith(".lang") || file.Name.EndsWith(".json"))
                fileList.Add(new StorageFileInfo(file));
        if (fileList.Count == 0)
        {
            ToastService.Notify("文件夹中没有任何语言文件 .lang 或 .json 类型格式文件", NotificationType.Error);
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Load Storage Files",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            Content = new ConfirmDialog
            {
                DataContext = LanguageChoice
            }
        };

        var confirm = false;

        dialog.Opened += DialogLoaded;
        dialog.PrimaryButtonClick += DialogConfirmed;
        dialog.Closed += DialogClosed;
        LanguageChoice.OnLoaded += LanguageLoaded;

        await dialog.ShowAsync();
        return;

        void DialogLoaded(object? sender, object? e)
        {
            dialog.Opened -= DialogLoaded;

            LanguageChoice.IsLoading = true;
            LanguageChoice.SetFileInfos(fileList);
            LanguageChoice.IsLoading = false;
            LanguageChoice.Success = true;
        }

        void DialogConfirmed(object? sender, object? e)
        {
            dialog.PrimaryButtonClick -= DialogConfirmed;
            confirm = true;
        }

        async void DialogClosed(object? sender, object? e)
        {
            dialog.Closed -= DialogClosed;
            if (!confirm) return;

            OriginalText = await LanguageChoice.SelectOriginal!.String();
            if (LanguageChoice.SelectTranslated is null)
                TranslatedText = "";
            else
                TranslatedText = await LanguageChoice.SelectTranslated!.String();

            TabSelectedIndex = 1;
            _previewFileStatus = ImportFileMode.Folder;
        }

        void LanguageLoaded()
        {
            LanguageChoice.OnLoaded -= LanguageLoaded;
            dialog.IsPrimaryButtonEnabled = true;
        }
    }

    private async Task OpenOriginalFile()
    {
        ToastService.Notify("未实现");
    }

    private async Task OpenTranslatedFile()
    {
        ToastService.Notify("未实现");
    }

    public void ManualInputCommand()
    {
        TabSelectedIndex = 1;
        EnableDocument = true;
        _previewFileStatus = ImportFileMode.Manual;
    }

    private ImportFileMode _previewFileStatus = ImportFileMode.None;

    public void NextCommand()
    {
        if (string.IsNullOrEmpty(OriginalText) && string.IsNullOrEmpty(TranslatedText))
        {
            ToastService.Notify("没有任何文本待翻译");
            return;
        }

        ModDictionary dict;

        switch (_previewFileStatus)
        {
            case ImportFileMode.Github:
                var id = GithubLinkStatus.Identifier.Split("/");
                var cfid = id[^2];
                var modid = id[^1];
                var version = Helper.GetVersion(GithubLinkStatus.Version!);
                dict = new ModDictionary(cfid, modid, version);
                dict.LoadOriginalFile(OriginalText);
                dict.LoadTranslatedFile(TranslatedText);
                break;
            case ImportFileMode.Folder:
                if (Folder is null)
                {
                    ToastService.Notify("未选择文件夹", NotificationType.Error);
                    return;
                }

                dict = new ModDictionary(Folder.Name, Folder.Path.AbsolutePath,
                    OriginalText.Trim().StartsWith('{')
                        ? MinecraftVersion.Version1Dot16
                        : MinecraftVersion.Version1Dot12Dot2);
                dict.LoadOriginalFile(OriginalText);
                dict.LoadTranslatedFile(TranslatedText);
                break;
            case ImportFileMode.Manual:
                dict = new ModDictionary("Manual", "Manual",
                    OriginalText.Trim().StartsWith('{')
                        ? MinecraftVersion.Version1Dot16
                        : MinecraftVersion.Version1Dot12Dot2);
                dict.LoadOriginalFile(OriginalText);
                dict.LoadTranslatedFile(TranslatedText);
                if (dict.TextDictionary.Count == 0 ||
                    string.IsNullOrEmpty(dict.TextDictionary.First().Value.OriginalText))
                {
                    ToastService.Notify("输入文本无法解析", NotificationType.Error);
                    return;
                }

                break;
            default:
                throw new NotImplementedException();
        }

        NavigationService.NavigatePage(1, dict);
    }
}

internal class LanguageChoice : ViewModelBase
{
    public delegate void DownloadHandler();

    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool Success { get; set; }
    [Reactive] public bool Downloading { get; private set; }
    [Reactive] public List<IFileInfo> FileInfos { get; set; } = [];
    [Reactive] public IFileInfo? SelectOriginal { get; set; }
    [Reactive] public IFileInfo? SelectTranslated { get; set; }
    public event DownloadHandler? OnLoaded;

    public async void DownloadCommand()
    {
        Downloading = true;
        if (!Success)
        {
            ToastService.Notify("请先获取文件列表", NotificationType.Error);
            return;
        }

        if (SelectOriginal is null)
        {
            ToastService.Notify("请选择原文文件", NotificationType.Error);
            return;
        }

        var tasks = new List<Task<string>> { SelectOriginal!.String() };
        if (SelectTranslated is not null && SelectTranslated.Name != "None") tasks.Add(SelectTranslated!.String());

        await Task.WhenAll(tasks);

        Downloading = false;
        ToastService.Notify("下载完成");
        OnLoaded?.Invoke();
    }

    public void SetFileInfos(List<IFileInfo> fileInfos)
    {
        FileInfos = fileInfos;
        SelectTranslated = fileInfos.Find(x => x.Name.StartsWith("zh_cn."));
        if (SelectTranslated is null)
        {
            FileInfos.Insert(0, new GitHubFileInfo("None", ""));
            SelectTranslated = FileInfos[0];
        }

        SelectOriginal = fileInfos.Find(x => x.Name.StartsWith("en_us."));
        if (SelectOriginal is not null) return;
        SelectOriginal = fileInfos.Find(x => !x.Name.StartsWith("zh_cn."));
        ToastService.Notify(SelectOriginal is null ? "选取目标没有任何非中文文件" : $"未找到原文文件，已选择其他文件: {SelectOriginal.Name}");
    }
}

internal partial class GithubLinkStatus : ViewModelBase
{
    private readonly Regex _githubRegex = GenRegex();
    public string Identifier = "";

    [Reactive] public bool GithubStatus { get; set; }
    [Reactive] public string? Version { get; set; }
    [Reactive] public bool EndsWithLang { get; set; }

    public void CheckLink(string link)
    {
        link = link.Trim();
        var match = _githubRegex.Match(link);

        GithubStatus = match.Success;
        EndsWithLang = link.EndsWith("/lang");

        Version = match is { Success: true, Groups.Count: > 1 } ? match.Groups[1].Value : null;
        Identifier = match is { Success: true, Groups.Count: > 2 } ? match.Groups[2].Value : "";
    }

    public bool Passed()
    {
        return GithubStatus && EndsWithLang && Version is not null;
    }

    [GeneratedRegex(
        @"^https?://github\.com/.*?/projects/([^/]+)/assets/(.*)/lang$")]
    private static partial Regex GenRegex();
}

internal enum ImportFileMode
{
    None,
    Github,
    Folder,
    Manual,
    Review
}