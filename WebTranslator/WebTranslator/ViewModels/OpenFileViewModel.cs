using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class OpenFileViewModel : ViewModelBase
{
    [Reactive] public Notification? NotifyMsg { get; private set; }
    [Reactive] public DialogMsg? ShowDialog { get; private set; }
    [Reactive] public string GithubLink { get; set; } = "";
    [Reactive] public string GithubLinkEnStatus { get; set; } = "未获取";
    public string EnText { get; set; } = "";
    [Reactive] public string GithubLinkZhStatus { get; set; } = "未获取";
    public string ZhText { get; set; } = "";
    [Reactive] public string OpenFileEnStatus { get; set; } = "未打开文件";
    public string FileZhText { get; set; } = "";
    [Reactive] public string OpenFileZhStatus { get; set; } = "未打开文件";
    public string FileEnText { get; set; } = "";
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
    private bool CheckJsonText(string en, string zh, out JsonReader? json)
    {
        if (string.IsNullOrEmpty(en))
        {
            Notify("错误", "英文文件内容为空", NotificationType.Error);
            json = null;
            return false;
        }
        json = new JsonReader(en);
        if (string.IsNullOrEmpty(zh))
            Notify("提示", "中文文件内容为空，将自动创建空文件", NotificationType.Information);
        zh = "{}";
        json.SetZhText(zh);
        return true;
    }

    public void ShowDialogCommand(string s)
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

    public void OpenFileCommand()
    {
        Notify("提示", "暂不支持打开文件", NotificationType.Information);
    }
    
    public void OpenFileDialogCommand(string s)
    {
        switch (s)
        {
            case "en_us":
                Dialog(s, FileEnText);
                break;
            case "zh_cn":
                Dialog(s, FileZhText);
                break;
        }
    }

    private void Notify(string title, string content, NotificationType type)
    {
        NotifyMsg = new Notification(title, content, type);
    }
    
    private void Dialog(string title, string content)
    {
        ShowDialog = new DialogMsg(title, content);
    }
}

public class DialogMsg
{
    public string Title { get; }
    public string Content { get;  }
    
    public DialogMsg(string title, string content)
    {
        Title = title;
        Content = content;
    }
}