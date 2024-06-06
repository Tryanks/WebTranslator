using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using WebTranslator.Interfaces;

namespace WebTranslator.Models;

public class StorageFileInfo(IStorageItem? item) : IFileInfo
{
    private string? Content { get; set; }
    public string Name { get; } = item?.Name ?? "";

    public async Task<string> String()
    {
        if (item is null) return "";
        var path = item.Path.AbsolutePath;
        Content ??= await File.ReadAllTextAsync(path);
        return Content;
    }
}