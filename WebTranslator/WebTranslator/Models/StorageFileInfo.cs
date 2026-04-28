using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using WebTranslator.Interfaces;

namespace WebTranslator.Models;

public class StorageFileInfo(IStorageItem? item) : IFileInfo
{
    private string? Content { get; set; }
    private IStorageFile? File { get; } = item as IStorageFile;
    public string Name { get; } = item?.Name ?? "";

    public async Task<string> String()
    {
        if (File is null) return "";
        if (Content is not null) return Content;

        await using var stream = await File.OpenReadAsync();
        using var reader = new StreamReader(stream);
        Content = await reader.ReadToEndAsync();
        return Content;
    }
}
