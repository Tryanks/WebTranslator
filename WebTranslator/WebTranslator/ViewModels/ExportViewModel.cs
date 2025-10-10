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
    public TextDocument DiffDocument { get; set; } = new();

    public string FileName { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = "zh_cn.json";
    public string FormatName { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = "JSON";
    public int TextLength { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public bool HasDiff { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public bool AutoSaveToOrigin { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); if (value) _ = SaveToOriginCommand(); } }
    public string? OriginFolderPath { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public bool IsLocalFolder { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
        
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

    public async Task SaveToOriginCommand()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OriginFolderPath))
            {
                ToastService.Notify("未检测到原始文件夹，仅支持本地路径打开后保存到原文件", Avalonia.Controls.Notifications.NotificationType.Warning);
                return;
            }
            var target = System.IO.Path.Combine(OriginFolderPath!, FileName);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
            await File.WriteAllTextAsync(target, Document.Text ?? string.Empty);
            ToastService.Notify($"已保存到原文件: {target}", Avalonia.Controls.Notifications.NotificationType.Success);
        }
        catch (Exception e)
        {
            ToastService.Notify($"保存到原文件失败: {e.Message}", Avalonia.Controls.Notifications.NotificationType.Error);
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

        // Read context for origin folder and prepare diff
        OriginFolderPath = ProjectContextService.OriginFolderPath;
        IsLocalFolder = ProjectContextService.ImportMode == ImportFileMode.Folder && !string.IsNullOrWhiteSpace(OriginFolderPath);
        if (IsLocalFolder)
        {
            var target = System.IO.Path.Combine(OriginFolderPath!, FileName);
            if (File.Exists(target))
            {
                try
                {
                    var oldText = File.ReadAllText(target);
                    var diff = ComputeLineDiff(oldText, Document.Text ?? string.Empty);
                    DiffDocument.Text = diff;
                    HasDiff = !string.IsNullOrWhiteSpace(diff);
                }
                catch (Exception e)
                {
                    ToastService.Notify($"加载原文件以显示差异失败: {e.Message}");
                }
            }
            else
            {
                DiffDocument.Text = "(原文件不存在，将创建新文件)";
                HasDiff = true;
            }
            // Default to auto-save when opened from local folder
            AutoSaveToOrigin = true;
        }
        else
        {
            AutoSaveToOrigin = false;
            HasDiff = false;
            DiffDocument.Text = string.Empty;
        }
    }

    private static string ComputeLineDiff(string oldText, string newText)
    {
        // Very small and simple line-by-line diff; not optimal but sufficient for preview
        var oldLines = (oldText ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var newLines = (newText ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var max = Math.Max(oldLines.Length, newLines.Length);
        using var sw = new StringWriter();
        sw.WriteLine($"--- 原文件");
        sw.WriteLine($"+++ 导出内容");
        for (int i = 0; i < max; i++)
        {
            var o = i < oldLines.Length ? oldLines[i] : null;
            var n = i < newLines.Length ? newLines[i] : null;
            if (o == n)
            {
                continue; // skip unchanged lines to keep preview compact
            }
            if (o is not null) sw.WriteLine($"- {o}");
            if (n is not null) sw.WriteLine($"+ {n}");
        }
        return sw.ToString();
    }
}