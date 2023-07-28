using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using AvaloniaEdit.Document;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class OpenFileViewModel : ViewModelBase
{
    [Reactive] public Notification? notifyMsg { get; set; }
    [Reactive] public DialogMsg? showDialog { get; set; }
    [Reactive] public string GithubLink { get; set; } = "";
    [Reactive] public string GithubLinkEnStatus { get; set; } = "未获取";
    public string EnText { get; set; } = "";
    [Reactive] public string GithubLinkZhStatus { get; set; } = "未获取";
    public string ZhText { get; set; } = "";
    public void CheckGithubLink()
    {
        if (string.IsNullOrEmpty(GithubLink))
        {
            Notify("错误", "Github链接不可为空", NotificationType.Error);
            return;
        }
        if (!GithubLink.StartsWith("https://github.com") || !GithubLink.EndsWith("/lang"))
        {
            Notify("错误", "Github链接无效", NotificationType.Error);
            return;
        }
        GithubLink = Utils.GithubConvert(GithubLink);
        Task.Run(async () =>
        {
            GithubLinkEnStatus = "获取中...";
            EnText = await Utils.GetGithubText(GithubLink + "en_us.json");
            GithubLinkEnStatus = !string.IsNullOrEmpty(EnText) ? "获取成功" : "获取失败";
        });
        Task.Run(async () =>
        {
            GithubLinkZhStatus = "获取中...";
            ZhText = await Utils.GetGithubText(GithubLink + "zh_cn.json");
            GithubLinkZhStatus = !string.IsNullOrEmpty(ZhText) ? "获取成功" : "获取失败";
        });
    }
    [Reactive] public int SelectedIndex { get; set; }
    public void NextCommand()
    {
        switch (SelectedIndex)
        {
            case 0:
                // Github链接
                break;
            case 1:
                // 输入json
                break;
            case 2:
                // 选择文件
                break;
        }
    }

    // private bool CheckGithub(out JsonReader en, out JsonReader zh)
    // {
    //     
    // }
    //
    // private bool CheckJson(out JsonReader en, out JsonReader zh)
    // {
    //     
    // }
    //
    // private bool CheckOpenFile(out JsonReader en, out JsonReader zh)
    // {
    //     
    // }
    //
    private bool CheckJsonText(string en, string zh, out JsonReader? enReader, out JsonReader? zhReader)
    {
        if (string.IsNullOrEmpty(en))
        {
            Notify("错误", "英文文件内容为空", NotificationType.Error);
            enReader = null;
            zhReader = null;
            return false;
        }
        enReader = new JsonReader(en);
        zhReader = new JsonReader(zh);
        if (string.IsNullOrEmpty(zh))
            Notify("提示", "中文文件内容为空，将自动创建空文件", NotificationType.Information);
        return true;
    }

    public void DialogCommand(string s)
    {
        switch (s)
        {
            case "en_us":
                Dialog(s, EnText);
                break;
            case "zh_cn":
                Dialog(s, ZhText);
                break;
        }

    }
    
    private void Notify(string title, string content, NotificationType type)
    {
        notifyMsg = new Notification(title, content, type);
    }
    
    private void Dialog(string title, string content)
    {
        showDialog = new DialogMsg(title, content);
    }
}

public class DialogMsg
{
    public string Title { get; set; }
    public string Content { get; set; }
    
    public DialogMsg(string title, string content)
    {
        Title = title;
        Content = content;
    }
}