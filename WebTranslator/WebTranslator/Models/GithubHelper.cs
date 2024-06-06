using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebTranslator.Interfaces;

namespace WebTranslator.Models;

public static partial class GithubHelper
{
    public static string GithubConvert(string link)
    {
        var regex = MyRegex();
        var match = regex.Match(link);

        if (!match.Success) throw new ArgumentException("输入的URL不符合期望的GitHub仓库URL格式。");

        var owner = match.Groups["owner"].Value;
        var repo = match.Groups["repo"].Value;
        var branch = match.Groups["branch"].Value;
        var path = match.Groups["path"].Value;

        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}?ref={branch}";

        return apiUrl;
    }

    public static async Task<List<IFileInfo>> GetLanguageFilesAsync(string apiUrl)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "CFPATools WebTranslator");

        var response = await httpClient.GetStringAsync(apiUrl);
        var allFiles = JArray.Parse(response);

        return
        [
            ..(from JObject? file in allFiles
                where file["type"].ToString() == "file"
                let name = file["name"].ToString()
                let downloadUrl = file["download_url"].ToString()
                select new GitHubFileInfo(name, downloadUrl)).ToList()
        ];
    }

    public static async Task<string> GetGithubTextAsync(string link)
    {
        if (string.IsNullOrEmpty(link)) return "";
        var client = new HttpClient();
        // 添加超时
        client.Timeout = TimeSpan.FromSeconds(10);
        // 当 404 时直接返回空字符串
        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(link);
            if (!response.IsSuccessStatusCode) return "{}";
        }
        catch (Exception)
        {
            return "{}";
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = Encoding.UTF8.GetString(bytes);
        return text;
    }

    public static string GithubGetCfId(string link)
    {
        return "";
    }

    public static string GithubGetModId(string link)
    {
        return "";
    }

    [GeneratedRegex(@"^https://github\.com/(?<owner>[^/]+)/(?<repo>[^/]+)/(blob|tree)/(?<branch>[^/]+)/(?<path>.*)$")]
    private static partial Regex MyRegex();
}

public class GitHubFileInfo(string name, string url) : IFileInfo
{
    private string DownloadUrl { get; } = url;
    private string? Content { get; set; }
    public string Name { get; } = name;

    public async Task<string> String()
    {
        if (string.IsNullOrEmpty(DownloadUrl)) return "";
        if (Content is not null) return Content;
        var url = "https://mirror.ghproxy.com/" +
                  (DownloadUrl.StartsWith("https://") ? DownloadUrl.Remove(0, "https://".Length) : DownloadUrl);
        Content ??= await GithubHelper.GetGithubTextAsync(url);
        return Content;
    }
}