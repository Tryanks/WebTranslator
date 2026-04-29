using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebTranslator.Services;

internal sealed partial class BrowserHostedDictionaryRemoteSource : IDictionaryRemoteSource
{
    public async Task<DictionaryDownload> DownloadAsync(IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        using var client = DictionaryService.CreateClient();
        var url = GetDictionaryUrl();
        string json;
        try
        {
            json = await DictionaryService.DownloadStringAsync(client, url, progress, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException("当前站点未发布词典文件 Dict-Mini.json，请重新部署 Browser 构建。", ex);
        }

        return new DictionaryDownload(
            json,
            "WebTranslator 在线词典",
            null,
            Encoding.UTF8.GetByteCount(json));
    }

    [JSImport("globalThis.webTranslatorDictionary.dictionaryUrl")]
    private static partial string GetDictionaryUrl();
}
