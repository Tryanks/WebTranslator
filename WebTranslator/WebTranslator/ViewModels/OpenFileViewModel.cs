using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        this.WhenAnyValue(x => x.GithubLink).Subscribe(link => { GithubLinkStatus.CheckLink(link); });
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

        var confirm = true;

        dialog.Opened += DialogLoaded;
        dialog.PrimaryButtonClick += DialogConfirmed;
        dialog.Closed += DialogClosed;

        await dialog.ShowAsync();
        return;

        async void DialogLoaded(object? sender, object? e)
        {
            dialog.Opened -= DialogLoaded;

            GithubLanguageChoice.IsLoading = true;

            ToastService.Notify("提示", "正在获取文件列表，请稍后");
            var infos = await GithubHelper.GetLanguageFilesAsync(link);
            if (infos.Count == 0)
            {
                ToastService.Notify("错误", "获取到的文件列表为空，请确定存在语言文件", NotificationType.Error);
                GithubLanguageChoice.IsLoading = false;
                return;
            }

            ToastService.Notify("提示", "获取文件列表成功");

            dialog.IsPrimaryButtonEnabled = true;
            GithubLanguageChoice.SetGithubFileInfos(infos);
            GithubLanguageChoice.IsLoading = false;
            GithubLanguageChoice.Success = true;
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

            // GithubConfirmCommand();

            OriginalText = await GithubLanguageChoice.SelectOriginal!.Value.Content();
            TranslatedText = await GithubLanguageChoice.SelectTranslated!.Value.Content();
            TabSelectedIndex = 1;
        }
    }

    public async void GithubConfirmCommand()
    {
        if (!GithubLanguageChoice.Success)
        {
            ToastService.Notify("错误", "请先获取文件列表", NotificationType.Error);
            return;
        }

        if (GithubLanguageChoice.SelectOriginal is null || GithubLanguageChoice.SelectTranslated is null)
        {
            ToastService.Notify("错误", "请选择原文和译文文件", NotificationType.Error);
            return;
        }

        OriginalText = await GithubLanguageChoice.SelectOriginal!.Value.Content();
        TranslatedText = await GithubLanguageChoice.SelectTranslated!.Value.Content();

        TabSelectedIndex = 1;
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
        ToastService.Notify("提示", "暂不支持打开" + word);
    }

    public void ManualInputCommand()
    {
        TabSelectedIndex = 1;
        EnableDocument = true;
    }
}

internal class LanguageChoice : ViewModelBase
{
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool Success { get; set; }
    [Reactive] public List<GitHubFileInfo> FileInfos { get; set; } = [];
    [Reactive] public GitHubFileInfo? SelectOriginal { get; set; }
    [Reactive] public GitHubFileInfo? SelectTranslated { get; set; }

    public void ConfirmCommand()
    {
        ToastService.Notify("title", "Confirm Command");
    }

    public void SetGithubFileInfos(List<GitHubFileInfo> fileInfos)
    {
        FileInfos = fileInfos;
        SelectTranslated = fileInfos.Find(x => x.Name.StartsWith("zh_cn."));
        SelectOriginal = fileInfos.Find(x => x.Name.StartsWith("en_us."));
        if (SelectOriginal is not null) return;
        SelectOriginal = fileInfos.Find(x => !x.Name.StartsWith("zh_cn."));
        if (SelectOriginal is null)
            ToastService.Notify("警告", "目标仓库没有任何非中文文件");
        else
            ToastService.Notify("提示", $"未找到原文文件，已选择其他文件: {SelectOriginal.Value.Name}");
    }
}

internal partial class GithubLinkStatus : ViewModelBase
{
    private readonly Regex _githubRegex = GenRegex();

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
    }

    public bool Passed()
    {
        return GithubStatus && EndsWithLang && Version is not null;
    }

    [GeneratedRegex(
        @"^https?://github\.com/.*?/projects/([^/]+)/assets/.*/lang$")]
    private static partial Regex GenRegex();
}