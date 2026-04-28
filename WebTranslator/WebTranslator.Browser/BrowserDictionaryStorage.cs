using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using WebTranslator.Services;

internal partial class BrowserDictionaryStorage : IDictionaryStorage
{
    public string DisplayLocation => "浏览器本地存储";
    public bool CanReadSynchronously => false;

    public string? ReadText()
    {
        return null;
    }

    public Task<string?> ReadTextAsync()
    {
        return ReadAsync();
    }

    public Task WriteTextAsync(string text)
    {
        return WriteAsync(text);
    }

    public Task DeleteAsync()
    {
        return DeleteAsyncCore();
    }

    public DateTimeOffset? GetLastWriteTime()
    {
        return null;
    }

    [JSImport("globalThis.webTranslatorDictionary.read")]
    private static partial Task<string?> ReadAsync();

    [JSImport("globalThis.webTranslatorDictionary.write")]
    private static partial Task WriteAsync(string text);

    [JSImport("globalThis.webTranslatorDictionary.remove")]
    private static partial Task DeleteAsyncCore();
}
