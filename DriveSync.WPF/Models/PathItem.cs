﻿namespace DriveSync.Models;

public enum ItemStatus
{
    ExistsAndEqual,
    ExistsButDifferent,
    ExistsWithDifferentName,
    DoesNotExist
}

public enum ItemType
{
    Folder,
    File,
    Android,
    Archive,
    Audio,
    Code,
    DiskImage,
    Executable,
    Font,
    Image,
    MarkupLanguage,
    PDF,
    Presentation,
    Spreadsheet,
    System,
    Text,
    Video
}

public class PathItem
{
    public DirectoryInfo Item { get; set; }
    public bool IsFile { get; set; }
    public ItemStatus Status { get; set; }
    public ItemType Type { get; set; }
    public string DifferentPath { get; set; }
}
