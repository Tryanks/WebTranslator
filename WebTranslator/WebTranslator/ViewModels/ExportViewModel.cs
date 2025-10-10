using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using ReactiveUI;
using WebTranslator.Models;
using WebTranslator.Services;

namespace WebTranslator.ViewModels;

public class ExportViewModel : ViewModelBase
{
    public TextDocument Document { get; set; } = new();

    public string FileName { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = "zh_cn.json";
    public string FormatName { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = "JSON";
    public int TextLength { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
        
    public void CopyCommand()
    {
        try
        {
            Application.Current?.Clipboard?.SetTextAsync(Document.Text);
            ToastService.Notify("已复制到剪贴板");
        }
        catch (Exception e)
        {
            ToastService.Notify($"复制失败: {e.Message}", Avalonia.Controls.Notifications.NotificationType.Error);
        }
    }

    public void CopyFileNameCommand()
    {
        try
        {
            Application.Current?.Clipboard?.SetTextAsync(FileName);
            ToastService.Notify("文件名已复制");
        }
        catch (Exception e)
        {
            ToastService.Notify($"复制失败: {e.Message}", Avalonia.Controls.Notifications.NotificationType.Error);
        }
    }

    public async Task SaveAsCommand()
    {
        try
        {
            if (!FilePickerService.CanSave)
            {
                ToastService.Notify("当前平台不支持保存", Avalonia.Controls.Notifications.NotificationType.Warning);
                return;
            }

            var file = await FilePickerService.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = FileName,
                Title = "另存为",
                DefaultExtension = Path.GetExtension(FileName),
                FileTypeChoices =
                [
                    new FilePickerFileType("JSON 语言文件") { Patterns = ["*.json"] },
                    new FilePickerFileType("Legacy .lang 文件") { Patterns = ["*.lang"] }
                ]
            });

            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(Document.Text);
            await writer.FlushAsync();

            ToastService.Notify("导出成功", Avalonia.Controls.Notifications.NotificationType.Success);
        }
        catch (Exception e)
        {
            ToastService.Notify($"导出失败: {e.Message}", Avalonia.Controls.Notifications.NotificationType.Error);
        }
    }

    public override void SetParameter(object? parameter)
    {
        if (parameter is not ModDictionary dict) return;
        Document.Text = dict.ToString();
        TextLength = Document.Text?.Length ?? 0;
        // Determine defaults from dictionary
        var langExt = dict.Format == LangFormat.Json ? ".json" : ".lang";
        FileName = dict.Version switch
        {
            MinecraftVersion.Version1Dot12Dot2 => "zh_cn" + langExt,
            _ => "zh_cn" + langExt
        };
        FormatName = dict.Format == LangFormat.Json ? "JSON" : "LANG";
    }
}