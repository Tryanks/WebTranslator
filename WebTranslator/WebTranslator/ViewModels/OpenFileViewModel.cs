using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
using WebTranslator.Controls;
using WebTranslator.Interfaces;
using WebTranslator.Models;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class OpenFileViewModel : ViewModelBase
{
    public OpenFileViewModel()
    {
        GithubLinkStatus.CheckLink(GithubLink);
    }

    public uint TabSelectedIndex { get => field; set => SetProperty(ref field, value); }
    public string GithubLink { get => field; set { if (!SetProperty(ref field, value)) return; GithubLinkStatus.CheckLink(value); } } =
#if DEBUG
        "https://github.com/CFPAOrg/Minecraft-Mod-Language-Package/tree/cf1a801466a901ec14a28c3b08dfe62f69c8db53/projects/1.20/assets/adorn-for-forge/adorn/lang";
#else
        "";
#endif
    internal GithubLinkStatus GithubLinkStatus { get => field; set => SetProperty(ref field, value); } = new();
    internal LanguageChoice LanguageChoice { get => field; set => SetProperty(ref field, value); } = new();
    public bool EnableDocument { get => field; set => SetProperty(ref field, value); }
    public TextDocument OriginalDocument { get => field; set => SetProperty(ref field, value); } = new();
    public TextDocument TranslatedDocument { get => field; set => SetProperty(ref field, value); } = new();

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
        ProjectContextService.SetGithub();
    }

    public async void OpenFolderCommand() => await OpenFolder();

    public async void OpenOriginalFileCommand() => await OpenOriginalFile();

    public async void OpenTranslatedFileCommand() => await OpenTranslatedFile();

    private IStorageFolder? Folder { get; set; }
    private string? OriginFolderPath { get; set; }
    private string? ImportDisplayName { get; set; }

    private async Task OpenFolder()
    {
        var folders = await FilePickerService.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (folders.Count != 1) return;
        Folder = folders[0];
        OriginFolderPath = GetStoragePath(Folder) ?? Folder.Name;
        ImportDisplayName = Folder.Name;
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
            if (Folder is not null)
                ProjectContextService.SetFolder(Folder);
        }

        void LanguageLoaded()
        {
            LanguageChoice.OnLoaded -= LanguageLoaded;
            dialog.IsPrimaryButtonEnabled = true;
        }
    }

    private async Task OpenOriginalFile()
    {
        var files = await FilePickerService.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择原文语言文件",
            AllowMultiple = false,
            FileTypeFilter = LanguageFileTypes()
        });
        if (files.Count != 1) return;

        await LoadOriginalStorageFile(files[0]);
    }

    private async Task OpenTranslatedFile()
    {
        var files = await FilePickerService.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择译文语言文件",
            AllowMultiple = false,
            FileTypeFilter = LanguageFileTypes()
        });
        if (files.Count != 1) return;

        TranslatedText = await ReadStorageFileAsync(files[0]);
        TabSelectedIndex = 1;
        _previewFileStatus = ImportFileMode.Folder;
        ToastService.Notify($"已载入译文文件: {files[0].Name}");
    }

    public async Task OpenPathAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            if (Directory.Exists(path))
            {
                var originalPath = FindOriginalLanguageFile(path);
                if (originalPath is null)
                {
                    ToastService.Notify("启动路径中未找到 en_us.json 或 en_us.lang", NotificationType.Error);
                    return;
                }

                await LoadOriginalPath(originalPath);
                return;
            }

            if (File.Exists(path))
            {
                await LoadOriginalPath(path);
                return;
            }

            ToastService.Notify($"启动路径不存在: {path}", NotificationType.Error);
        }
        catch (Exception e)
        {
            ToastService.Notify($"启动路径打开失败: {e.Message}", NotificationType.Error);
        }
    }

    private async Task LoadOriginalStorageFile(IStorageFile file)
    {
        OriginalText = await ReadStorageFileAsync(file);

        var path = GetStoragePath(file);
        var folderPath = path is null ? null : Path.GetDirectoryName(path);
        var canUseLocalPath = !OperatingSystem.IsBrowser() && folderPath is not null;
        var translatedPath = canUseLocalPath && path is not null ? GetDefaultTranslatedPath(path) : null;
        TranslatedText = translatedPath is not null && File.Exists(translatedPath)
            ? await File.ReadAllTextAsync(translatedPath)
            : "";

        Folder = null;
        OriginFolderPath = folderPath ?? file.Name;
        ImportDisplayName = folderPath is null ? file.Name : Path.GetFileName(folderPath);
        TabSelectedIndex = 1;
        _previewFileStatus = ImportFileMode.Folder;
        if (!OperatingSystem.IsBrowser() && folderPath is not null)
            ProjectContextService.SetFolderPath(folderPath);
        else
            ProjectContextService.SetManual();

        ToastService.Notify(translatedPath is not null && File.Exists(translatedPath)
            ? $"已载入原文和译文: {Path.GetFileName(path)} / {Path.GetFileName(translatedPath)}"
            : $"已载入原文文件: {file.Name}");
    }

    private async Task LoadOriginalPath(string originalPath)
    {
        var folderPath = Path.GetDirectoryName(originalPath);
        if (folderPath is null)
        {
            ToastService.Notify("无法识别原文文件所在文件夹", NotificationType.Error);
            return;
        }

        OriginalText = await File.ReadAllTextAsync(originalPath);
        var translatedPath = GetDefaultTranslatedPath(originalPath);
        TranslatedText = translatedPath is not null && File.Exists(translatedPath)
            ? await File.ReadAllTextAsync(translatedPath)
            : "";

        Folder = null;
        OriginFolderPath = folderPath;
        ImportDisplayName = Path.GetFileName(folderPath);
        TabSelectedIndex = 1;
        _previewFileStatus = ImportFileMode.Folder;
        ProjectContextService.SetFolderPath(folderPath);

        ToastService.Notify(translatedPath is not null && File.Exists(translatedPath)
            ? $"已载入原文和译文: {Path.GetFileName(originalPath)} / {Path.GetFileName(translatedPath)}"
            : $"已载入原文文件: {Path.GetFileName(originalPath)}");
    }

    private static string? FindOriginalLanguageFile(string folderPath)
    {
        foreach (var fileName in new[] { "en_us.json", "en_us.lang" })
        {
            var path = Path.Combine(folderPath, fileName);
            if (File.Exists(path)) return path;
        }

        return Directory.EnumerateFiles(folderPath)
            .FirstOrDefault(x => x.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                                 x.EndsWith(".lang", StringComparison.OrdinalIgnoreCase));
    }

    private static string? GetDefaultTranslatedPath(string originalPath)
    {
        var folderPath = Path.GetDirectoryName(originalPath);
        if (folderPath is null) return null;
        var extension = Path.GetExtension(originalPath);
        return Path.Combine(folderPath, "zh_cn" + extension);
    }

    private static async Task<string> ReadStorageFileAsync(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static string? GetStoragePath(IStorageItem item)
    {
        try
        {
            return item.Path?.LocalPath;
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<FilePickerFileType> LanguageFileTypes()
    {
        return
        [
            new FilePickerFileType("Minecraft 语言文件") { Patterns = ["*.json", "*.lang"] },
            FilePickerFileTypes.All
        ];
    }

    public void ManualInputCommand()
    {
        TabSelectedIndex = 1;
        EnableDocument = true;
        _previewFileStatus = ImportFileMode.Manual;
        ProjectContextService.SetManual();
    }

    private ImportFileMode _previewFileStatus = ImportFileMode.None;

    public void NextCommand()
    {
        if (string.IsNullOrWhiteSpace(OriginalText))
        {
            ToastService.Notify("没有原文内容待翻译");
            return;
        }

        ModDictionary dict;
        try
        {
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
                    if (string.IsNullOrWhiteSpace(OriginFolderPath))
                    {
                        ToastService.Notify("未选择文件夹", NotificationType.Error);
                        return;
                    }

                    dict = new ModDictionary(ImportDisplayName ?? "Local", OriginFolderPath,
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
        }
        catch (Exception e)
        {
            ToastService.Notify($"输入文本无法解析: {e.Message}", NotificationType.Error);
            return;
        }

        NavigationService.NavigatePage(1, dict);
    }
}

internal class LanguageChoice : ViewModelBase
{
    public delegate void DownloadHandler();

    public bool IsLoading { get => field; set => SetProperty(ref field, value); }
    public bool Success { get => field; set => SetProperty(ref field, value); }
    public bool Downloading { get => field; private set => SetProperty(ref field, value); }
    public List<IFileInfo> FileInfos { get => field; set => SetProperty(ref field, value); } = [];
    public IFileInfo? SelectOriginal { get => field; set => SetProperty(ref field, value); }
    public IFileInfo? SelectTranslated { get => field; set => SetProperty(ref field, value); }
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

    public bool GithubStatus { get => field; set => SetProperty(ref field, value); }
    public string? Version { get => field; set => SetProperty(ref field, value); }
    public bool EndsWithLang { get => field; set => SetProperty(ref field, value); }

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
