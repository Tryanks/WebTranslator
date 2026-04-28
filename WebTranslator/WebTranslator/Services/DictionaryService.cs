using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebTranslator.Models;

namespace WebTranslator.Services;

public static class DictionaryService
{
    private const string ReleaseApiUrl = "https://api.github.com/repos/CFPATools/i18n-dict/releases/latest";
    private const string DictionaryAssetName = "Dict-Mini.json";
    private static Dictionary<string, List<string>>? Entries { get; set; }
    private static bool Initialized { get; set; }
    private static IDictionaryStorage Storage { get; set; } = new FileDictionaryStorage();
    private static DictionaryStatus Status { get; set; } =
        new(false, 0, "未安装", "", "", null, 0, Storage.DisplayLocation);

    public static void SetStorage(IDictionaryStorage storage)
    {
        Storage = storage;
        Entries = null;
        Initialized = false;
        Status = new DictionaryStatus(false, 0, "未安装", "", "", null, 0, storage.DisplayLocation);
    }

    public static DictionaryStatus GetStatus()
    {
        InitializeFromLocalStorage();
        return Status;
    }

    public static IReadOnlyList<string> GetTranslations(string originalText)
    {
        if (string.IsNullOrWhiteSpace(originalText)) return [];
        InitializeFromLocalStorage();
        if (Entries is null) return [];
        return Entries.TryGetValue(originalText, out var values) ? values : [];
    }

    public static async Task<DictionaryStatus> InitializeAsync()
    {
        if (Initialized) return Status;

        try
        {
            var json = await Storage.ReadTextAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                Initialized = true;
                Status = Status with { Installed = false, Message = "未安装", Location = Storage.DisplayLocation };
                return Status;
            }

            LoadJson(json, "本地词典", Storage.GetLastWriteTime());
            return Status;
        }
        catch
        {
            Initialized = true;
            Status = Status with { Installed = false, Message = "本地词典读取失败", Location = Storage.DisplayLocation };
            return Status;
        }
    }

    public static async Task<DictionaryStatus> DownloadLatestAsync(IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var releaseResponse = await client.GetAsync(ReleaseApiUrl, cancellationToken);
        releaseResponse.EnsureSuccessStatusCode();

        await using var releaseStream = await releaseResponse.Content.ReadAsStreamAsync(cancellationToken);
        var release = await JsonSerializer.DeserializeAsync(releaseStream,
            WebTranslatorJsonContext.Default.GitHubRelease, cancellationToken);
        var asset = release?.Assets.FirstOrDefault(x => x.Name == DictionaryAssetName);
        if (asset?.BrowserDownloadUrl is null)
            throw new InvalidOperationException($"最新 Release 中未找到 {DictionaryAssetName}。");

        var json = await DownloadStringAsync(client, asset.BrowserDownloadUrl, progress, cancellationToken);
        var entries = Parse(json);
        if (entries.Count == 0)
            throw new InvalidOperationException("下载的词典为空或格式不可识别。");

        await Storage.WriteTextAsync(json);
        ApplyEntries(entries, release?.Name ?? release?.TagName ?? "最新词典", release?.PublishedAt, asset.Size);
        return Status;
    }

    public static async Task<DictionaryStatus> ClearAsync()
    {
        await Storage.DeleteAsync();
        Entries = null;
        Initialized = true;
        Status = new DictionaryStatus(false, 0, "未安装", "", "", null, 0, Storage.DisplayLocation);
        return Status;
    }

    private static void InitializeFromLocalStorage()
    {
        if (Initialized) return;
        if (!Storage.CanReadSynchronously) return;
        try
        {
            var json = Storage.ReadText();
            if (!string.IsNullOrWhiteSpace(json))
                LoadJson(json, "本地词典", Storage.GetLastWriteTime());
            else
                Status = Status with { Installed = false, Message = "未安装", Location = Storage.DisplayLocation };
        }
        catch
        {
            Status = Status with { Installed = false, Message = "本地词典读取失败", Location = Storage.DisplayLocation };
        }

        Initialized = true;
    }

    private static void LoadJson(string json, string sourceName, DateTimeOffset? updatedAt)
    {
        var entries = Parse(json);
        ApplyEntries(entries, sourceName, updatedAt, json.Length);
    }

    private static void ApplyEntries(Dictionary<string, List<string>> entries, string sourceName,
        DateTimeOffset? updatedAt, long size)
    {
        Entries = entries;
        Initialized = true;
        Status = new DictionaryStatus(
            entries.Count > 0,
            entries.Count,
            entries.Count > 0 ? "已安装" : "未安装",
            sourceName,
            FormatSize(size),
            updatedAt,
            size,
            Storage.DisplayLocation);
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CFPATools WebTranslator");
        return client;
    }

    private static async Task<string> DownloadStringAsync(HttpClient client, string url, IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var length = response.Content.Headers.ContentLength;
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var memory = new MemoryStream(length is > 0 and <= int.MaxValue ? (int)length.Value : 0);
        var buffer = new byte[128 * 1024];
        long total = 0;

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            await memory.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            total += read;
            if (length is > 0) progress?.Report((double)total / length.Value);
        }

        progress?.Report(1);
        return System.Text.Encoding.UTF8.GetString(memory.ToArray());
    }

    private static Dictionary<string, List<string>> Parse(string json)
    {
        var result = new Dictionary<string, List<string>>();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return result;

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            var values = property.Value.ValueKind switch
            {
                JsonValueKind.Array => property.Value.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .OfType<string>()
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                JsonValueKind.String => [property.Value.GetString()!],
                _ => []
            };

            if (values.Count > 0) result[property.Name] = values;
        }

        return result;
    }

    private static string FormatSize(long bytes)
    {
        if (bytes <= 0) return "";
        string[] units = ["B", "KB", "MB", "GB"];
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.#} {units[unit]}";
    }
}

public interface IDictionaryStorage
{
    string DisplayLocation { get; }
    bool CanReadSynchronously { get; }
    string? ReadText();
    Task<string?> ReadTextAsync();
    Task WriteTextAsync(string text);
    Task DeleteAsync();
    DateTimeOffset? GetLastWriteTime();
}

public sealed record DictionaryStatus(
    bool Installed,
    int EntryCount,
    string Message,
    string SourceName,
    string SizeText,
    DateTimeOffset? UpdatedAt,
    long Size,
    string Location);

internal sealed class FileDictionaryStorage : IDictionaryStorage
{
    private static string DirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WebTranslator");

    private static string DictionaryPath => Path.Combine(DirectoryPath, "Dict-Mini.json");

    public string DisplayLocation => DictionaryPath;
    public bool CanReadSynchronously => !OperatingSystem.IsBrowser();

    public string? ReadText()
    {
        if (!CanReadSynchronously || !File.Exists(DictionaryPath)) return null;
        return File.ReadAllText(DictionaryPath);
    }

    public async Task<string?> ReadTextAsync()
    {
        if (!CanReadSynchronously || !File.Exists(DictionaryPath)) return null;
        return await File.ReadAllTextAsync(DictionaryPath);
    }

    public async Task WriteTextAsync(string text)
    {
        if (OperatingSystem.IsBrowser())
            throw new PlatformNotSupportedException("浏览器端需使用专用词典存储。");

        Directory.CreateDirectory(DirectoryPath);
        await File.WriteAllTextAsync(DictionaryPath, text);
    }

    public Task DeleteAsync()
    {
        if (!OperatingSystem.IsBrowser() && File.Exists(DictionaryPath))
            File.Delete(DictionaryPath);
        return Task.CompletedTask;
    }

    public DateTimeOffset? GetLastWriteTime()
    {
        if (OperatingSystem.IsBrowser() || !File.Exists(DictionaryPath)) return null;
        return File.GetLastWriteTime(DictionaryPath);
    }
}
