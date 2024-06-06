using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace WebTranslator.Services;

public static class FilePickerService
{
    private static IStorageProvider? Storage { get; set; }

    public static bool CanOpen => Storage!.CanOpen;
    public static bool CanSave => Storage!.CanSave;
    public static bool CanPickFolder => Storage!.CanPickFolder;

    public static void Set(TopLevel? level)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (Storage is not null) return;
        Storage = level.StorageProvider;
    }

    public static Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.OpenFilePickerAsync(options);
    }

    public static Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.SaveFilePickerAsync(options);
    }

    public static Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.OpenFolderPickerAsync(options);
    }

    public static Task<IStorageBookmarkFile?> OpenFileBookmarkAsync(string bookmark)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.OpenFileBookmarkAsync(bookmark);
    }

    public static Task<IStorageBookmarkFolder?> OpenFolderBookmarkAsync(string bookmark)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.OpenFolderBookmarkAsync(bookmark);
    }

    public static Task<IStorageFile?> TryGetFileFromPathAsync(Uri filePath)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.TryGetFileFromPathAsync(filePath);
    }

    public static Task<IStorageFolder?> TryGetFolderFromPathAsync(Uri folderPath)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.TryGetFolderFromPathAsync(folderPath);
    }

    public static Task<IStorageFolder?> TryGetWellKnownFolderAsync(WellKnownFolder wellKnownFolder)
    {
        ArgumentNullException.ThrowIfNull(Storage);
        return Storage.TryGetWellKnownFolderAsync(wellKnownFolder);
    }
}