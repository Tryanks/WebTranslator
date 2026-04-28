using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebTranslator.Models;

internal sealed class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = "";

    public string Name { get; set; } = "";

    [JsonPropertyName("published_at")]
    public DateTimeOffset? PublishedAt { get; set; }

    public List<GitHubReleaseAsset> Assets { get; set; } = [];
}

internal sealed class GitHubReleaseAsset
{
    public string Name { get; set; } = "";
    public long Size { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = "";
}
