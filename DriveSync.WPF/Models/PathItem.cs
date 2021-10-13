using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DriveSync.Models
{
    public class PathItem
    {
        public DirectoryInfo Item { get; set; }
        public bool IsFile { get; set; }
        public bool Exists { get; set; }
        public bool IsDifferent { get; set; }
        public bool IsPathCorrect { get; set; }
        public string RealPath { get; set; }
    }
}
