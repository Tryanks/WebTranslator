using System;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace WebTranslator.Services;

public static class FilePickerService
{
    private static IStorageProvider? Storage { get; set; }

    public static void Set(TopLevel? level)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (Storage is not null) return;
        Storage = level.StorageProvider;
    }
}