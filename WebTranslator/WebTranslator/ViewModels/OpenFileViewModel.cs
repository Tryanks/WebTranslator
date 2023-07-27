using System.Threading.Tasks;
using AvaloniaEdit.Document;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class OpenFileViewModel : ViewModelBase
{
    [Reactive] public string GithubLink { get; set; } = "";
    [Reactive] public string GithubLinkEnStatus { get; set; } = "未获取";
    public string EnText { get; set; } = "";
    [Reactive] public string GithubLinkZhStatus { get; set; } = "未获取";
    public string ZhText { get; set; } = "";
    public void CheckGithubLink()
    {
        if (string.IsNullOrEmpty(GithubLink)) return;
        if (!GithubLink.StartsWith("https://github.com") || !GithubLink.EndsWith("/lang"))
        {
            GithubLinkEnStatus = "链接无效";
            GithubLinkZhStatus = "链接无效";
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
}