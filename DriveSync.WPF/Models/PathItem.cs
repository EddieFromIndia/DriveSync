using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DriveSync.Models
{
    public enum ItemStatus
    {
        ExistsAndEqual,
        ExistsButDifferent,
        ExistsElsewhere,
        DoesNotExist
    }
    public class PathItem
    {
        public DirectoryInfo Item { get; set; }
        public bool IsFile { get; set; }
        public ItemStatus Status { get; set; }
        public string RealPath { get; set; }
    }
}
