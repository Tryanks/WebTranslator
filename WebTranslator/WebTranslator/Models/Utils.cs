using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebTranslator.Models;

public static partial class Utils
{
    public static string FileLinkConvert(string link)
    {
        if (string.IsNullOrEmpty(link)) return "";
        if (!link.EndsWith("json") || !link.EndsWith("lang")) return "";
        return link.Replace("https://github.com", "https://raw.githubusercontent.com")
            .Replace("/raw/", "/");
    }
    
    public static string GithubConvert(string link)
    {
        if (string.IsNullOrEmpty(link)) return "";
        link = link.Replace("https://github.com", "https://raw.githubusercontent.com")
            .Replace("/blob/", "/")
            .Replace("/tree/", "/");
        if (link.EndsWith("/lang")) link += "/";
        return link;
    }
    
    public static async Task<string> GetGithubText(string link)
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

    public static async Task<string> Request(string link)
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
            if (!response.IsSuccessStatusCode) return "";
        }
        catch (Exception)
        {
            return "";
        }
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var text = Encoding.UTF8.GetString(bytes);
        return text;
    }

    private static readonly Regex CfIdRegex = CfRegex();

    private static readonly Regex ModIdRegex = ModRegex();

    public static string GithubGetCfId(string link)
    {
        return CfIdRegex.Match(link).Groups[1].Value;
    }
    
    public static string GithubGetModId(string link)
    {
        return ModIdRegex.Match(link).Groups[1].Value;
    }

    [GeneratedRegex("assets/([^/]*)(?=/)")]
    private static partial Regex CfRegex();
    [GeneratedRegex("assets/[^/]*/([^/]*)(?=/)")]
    private static partial Regex ModRegex();
}