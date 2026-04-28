using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        _ = RefreshStatusAsync();
    }

    public bool IsBusy { get => field; set { if (!SetProperty(ref field, value)) return; RaisePropertyChanged(nameof(CanStartOperation)); } }
    public bool CanStartOperation => !IsBusy;
    public bool IsDictionaryInstalled { get => field; set => SetProperty(ref field, value); }
    public string DictionaryStatusText { get => field; set => SetProperty(ref field, value); } = "检查中";
    public string DictionarySourceText { get => field; set => SetProperty(ref field, value); } = "未安装";
    public string DictionaryEntryCountText { get => field; set => SetProperty(ref field, value); } = "0 条";
    public string DictionarySizeText { get => field; set => SetProperty(ref field, value); } = "-";
    public string DictionaryUpdatedAtText { get => field; set => SetProperty(ref field, value); } = "-";
    public string DictionaryLocationText { get => field; set => SetProperty(ref field, value); } = "";
    public double DownloadProgress { get => field; set => SetProperty(ref field, value); }
    public bool HasDownloadProgress { get => field; set => SetProperty(ref field, value); }

    public async Task RefreshCommand()
    {
        await RefreshStatusAsync();
    }

    public async Task DownloadDictionaryCommand()
    {
        if (IsBusy) return;
        IsBusy = true;
        HasDownloadProgress = true;
        DownloadProgress = 0;
        DictionaryStatusText = "正在下载";

        try
        {
            var progress = new Progress<double>(value => DownloadProgress = value * 100);
            var status = await DictionaryService.DownloadLatestAsync(progress);
            ApplyStatus(status);
            ToastService.Notify("词典已更新", NotificationType.Success);
        }
        catch (Exception ex)
        {
            DictionaryStatusText = "下载失败";
            ToastService.Notify($"词典下载失败：{ex.Message}", NotificationType.Error);
        }
        finally
        {
            IsBusy = false;
            HasDownloadProgress = false;
        }
    }

    public async Task ClearDictionaryCommand()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            ApplyStatus(await DictionaryService.ClearAsync());
            ToastService.Notify("已移除本地词典", NotificationType.Success);
        }
        catch (Exception ex)
        {
            ToastService.Notify($"移除词典失败：{ex.Message}", NotificationType.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshStatusAsync()
    {
        ApplyStatus(await DictionaryService.InitializeAsync());
    }

    private void ApplyStatus(DictionaryStatus status)
    {
        IsDictionaryInstalled = status.Installed;
        DictionaryStatusText = status.Message;
        DictionarySourceText = string.IsNullOrWhiteSpace(status.SourceName) ? "未安装" : status.SourceName;
        DictionaryEntryCountText = $"{status.EntryCount:N0} 条";
        DictionarySizeText = string.IsNullOrWhiteSpace(status.SizeText) ? "-" : status.SizeText;
        DictionaryUpdatedAtText = status.UpdatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
        DictionaryLocationText = status.Location;
        DownloadProgress = 0;
    }
}
