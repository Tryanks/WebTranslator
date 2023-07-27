using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebTranslator.Models;

public static class Utils
{
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
}