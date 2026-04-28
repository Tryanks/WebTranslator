using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebTranslator.Models;

[JsonSourceGenerationOptions(
    IncludeFields = true,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ModDictionary))]
[JsonSerializable(typeof(ReviewPrMsg))]
[JsonSerializable(typeof(List<GitHubContentItem>))]
internal partial class WebTranslatorJsonContext : JsonSerializerContext;
