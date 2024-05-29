using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Controls;
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
        "https://github.com/CFPAOrg/Minecraft-Mod-Language-Package/tree/main/projects/1.20-fabric/assets/better-than-bunnies-fabric/betterthanbunnies/lang";
#else
        "";
#endif
    [Reactive] internal GithubLinkStatus GithubLinkStatus { get; set; } = new();
    [Reactive] internal LanguageChoice GithubLanguageChoice { get; set; } = new();
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
        GithubLanguageChoice.IsLoading = false;
        GithubLanguageChoice.Success = false;

        var dialog = new ContentDialog
        {
            Title = "Load Github Files",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = false,
            Content = new ConfirmDialog
            {
                DataContext = GithubLanguageChoice
            }
        };

        var confirm = false;

        dialog.Opened += DialogLoaded;
        dialog.PrimaryButtonClick += DialogConfirmed;
        dialog.Closed += DialogClosed;
        GithubLanguageChoice.OnDownloaded += DialogDownloaded;

        await dialog.ShowAsync();
        return;

        async void DialogLoaded(object? sender, object? e)
        {
            dialog.Opened -= DialogLoaded;

            GithubLanguageChoice.IsLoading = true;

            ToastService.Notify("正在获取文件列表，请稍后");
            var infos = await GithubHelper.GetLanguageFilesAsync(link);
            if (infos.Count == 0)
            {
                ToastService.Notify("获取到的文件列表为空，请确定存在语言文件", NotificationType.Error);
                GithubLanguageChoice.IsLoading = false;
                return;
            }

            ToastService.Notify("获取文件列表成功");

            dialog.IsPrimaryButtonEnabled = false;
            GithubLanguageChoice.SetGithubFileInfos(infos);
            GithubLanguageChoice.IsLoading = false;
            GithubLanguageChoice.Success = true;
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
            GithubLanguageChoice.OnDownloaded -= DialogDownloaded;
            dialog.IsPrimaryButtonEnabled = true;
        }
    }

    private async void GithubConfirmCommand()
    {
        if (!GithubLanguageChoice.Success)
        {
            ToastService.Notify("请先获取文件列表", NotificationType.Error);
            return;
        }

        if (GithubLanguageChoice.SelectOriginal is null || GithubLanguageChoice.SelectTranslated is null)
        {
            ToastService.Notify("请选择原文和译文文件", NotificationType.Error);
            return;
        }

        OriginalText = await GithubLanguageChoice.SelectOriginal!.String();
        TranslatedText = await GithubLanguageChoice.SelectTranslated!.String();

        TabSelectedIndex = 1;
        PreviewFileStatus = ImportFileMode.Github;
    }

    public void OpenFileCommand(string s)
    {
        var word = s switch
        {
            "Folder" => "文件夹",
            "Original" => "原文文件",
            "Translated" => "译文文件",
            _ => "未知文件"
        };
        ToastService.Notify("暂不支持打开" + word);
#if true
        return;
#endif
        PreviewFileStatus = ImportFileMode.Folder;
    }

    public void ManualInputCommand()
    {
        TabSelectedIndex = 1;
        EnableDocument = true;
        PreviewFileStatus = ImportFileMode.Manual;
    }

    internal ImportFileMode PreviewFileStatus = ImportFileMode.None;

    public void NextCommand()
    {
        if (string.IsNullOrEmpty(OriginalText) && string.IsNullOrEmpty(TranslatedText))
        {
            ToastService.Notify("没有任何文本待翻译");
            return;
        }

        ModDictionary dict;

        switch (PreviewFileStatus)
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
    [Reactive] public bool Downloading { get; set; }
    [Reactive] public List<GitHubFileInfo> FileInfos { get; set; } = [];
    [Reactive] public GitHubFileInfo? SelectOriginal { get; set; }
    [Reactive] public GitHubFileInfo? SelectTranslated { get; set; }
    public event DownloadHandler? OnDownloaded;

    public async void DownloadCommand()
    {
        Downloading = true;
        if (!Success)
        {
            ToastService.Notify("请先获取文件列表", NotificationType.Error);
            return;
        }

        if (SelectOriginal is null || SelectTranslated is null)
        {
            ToastService.Notify("请选择原文和译文文件", NotificationType.Error);
            return;
        }

        var task1 = SelectOriginal!.String();
        var task2 = SelectTranslated!.String();

        await Task.WhenAll(task1, task2);

        Downloading = false;
        ToastService.Notify("下载完成");
        OnDownloaded?.Invoke();
    }

    public void SetGithubFileInfos(List<GitHubFileInfo> fileInfos)
    {
        FileInfos = fileInfos;
        SelectTranslated = fileInfos.Find(x => x.Name.StartsWith("zh_cn."));
        SelectOriginal = fileInfos.Find(x => x.Name.StartsWith("en_us."));
        if (SelectOriginal is not null) return;
        SelectOriginal = fileInfos.Find(x => !x.Name.StartsWith("zh_cn."));
        ToastService.Notify(SelectOriginal is null ? "目标仓库没有任何非中文文件" : $"未找到原文文件，已选择其他文件: {SelectOriginal.Name}");
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