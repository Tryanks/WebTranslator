using System;
using Avalonia.Platform.Storage;
using WebTranslator.Models;

namespace WebTranslator.Services;

public static class ProjectContextService
{
    public static ImportFileMode ImportMode { get; private set; } = ImportFileMode.None;
    public static string? OriginFolderPath { get; private set; }

    public static void SetFolder(IStorageFolder folder)
    {
        OriginFolderPath = OperatingSystem.IsBrowser() ? null : folder.Path?.LocalPath;
        ImportMode = ImportFileMode.Folder;
    }

    public static void SetFolderPath(string folderPath)
    {
        OriginFolderPath = folderPath;
        ImportMode = ImportFileMode.Folder;
    }

    public static void SetGithub()
    {
        OriginFolderPath = null;
        ImportMode = ImportFileMode.Github;
    }

    public static void SetManual()
    {
        OriginFolderPath = null;
        ImportMode = ImportFileMode.Manual;
    }

    public static void Clear()
    {
        OriginFolderPath = null;
        ImportMode = ImportFileMode.None;
    }
}
