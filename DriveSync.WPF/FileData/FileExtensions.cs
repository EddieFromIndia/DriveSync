namespace DriveSync;

public class FileExtensions
{
    public static List<string> Android = new()
        { "apk", "abb" };

    public static List<string> Archive = new()
        { "7z", "ace", "arj", "bz2", "cab", "gz", "gzi", "gzip", "jar", "lzh", "rar", "tar", "uue", "z", "zip" };

    public static List<string> Audio = new()
        { "3ga", "8svx", "aa", "aac", "aax", "aaxc", "ac3", "act", "aif", "aifc", "aiff", "alac", "amr", "aob",
                "ape", "at3", "au", "awb", "cda", "dss", "dvf", "f4a", "flac", "gsm", "iklax", "ivs", "m4a", "m4b", "m4p", "mmf", "mogg",
                "mp2", "mp3", "mpc", "msv", "nmf", "oga", "ogg", "opus", "ra", "rf64", "tta", "wav", "wma" };

    public static List<string> Code = new()
        { "abap", "ada", "c", "cbl", "cls", "cob", "cpp", "cs", "css", "dart", "f", "f90", "for", "go", "gml", "groovy",
                "ipynb", "java", "jl", "js", "json", "jsp", "jspx", "jsx", "kt", "kts", "lua", "m", "mat", "pas", "php", "php3", "pl", "pp", "py", "r",
                "rb", "rs", "sc", "scala", "swift", "ts", "tsx", "yaml" };

    public static List<string> DiskImage = new()
        { "dmg", "iso", "smi" };

    public static List<string> Executable = new()
        { "exe", "msi" };

    public static List<string> Font = new()
        { "afm", "cff", "dfont", "eot", "fon", "ntf", "otf", "pfa", "pfb", "pfm", "ps", "pt3", "suit", "t11", "t42",
                "tfm", "ttc", "ttf", "ufo", "woff", "woff2" };

    public static List<string> Image = new()
        { "3fr", "arw", "avci", "avcs", "avif", "avifs", "bmp", "cr2", "dc3", "dcm", "dib", "dic", "dng", "eps", "gif", "heic", "heics", "heif", "heifs", "icb", "ico",
                "j2c", "j2k", "jp2", "jpe", "jpeg", "jpc", "jpf", "jpg", "jps", "jpx", "mpo", "nef", "orf", "pam", "pbm", "pcx", "pdd", "pef", "pfm", "pgm", "png",
                "pnm", "ppm", "psb", "psd", "psdt", "pxr", "raf", "raw", "rle", "rw2", "sct", "sr2", "svg", "tga", "tif", "tiff", "vda", "vst", "xbm", "xpm" };

    public static List<string> MarkupLanguage = new()
        { "asp", "aspx", "dhtml", "htm", "html", "htmls", "jsx", "rhtml", "sgml", "xaml", "xhtml", "xml" };

    public static List<string> PDF = new()
        { "pdf" };

    public static List<string> Presentation = new()
        { "odp", "otp", "pot", "potm", "potx", "pps", "ppsm", "ppsx", "ppt", "pptx" };

    public static List<string> Spreadsheet = new()
        { "xlr", "xls", "xlsb", "xlsm", "xlsx", "xlt", "xltm", "xltx", "xlw" };

    public static List<string> System = new()
        { "bat", "btm", "cmd", "com", "command", "dll" };

    public static List<string> Text = new()
        { "doc", "docm", "docx", "dotm", "dotx", "eml", "ini", "md", "odt", "rtf", "txt", "xps" };

    public static List<string> Video = new()
        { "3g2", "3gp", "amv", "asf", "avi", "drc", "f4v", "flv", "gifv", "hevc", "m4v", "mkv", "m2ts", "m2v", "m4p", "m4v",
                "mov", "mp4", "mpe", "mpeg", "mpg", "mpv", "mts", "mxf", "nsv", "ogv", "rm", "rmvb", "ts", "viv", "vob", "webm", "wmv" };

    private static readonly List<string> HashSearchableExtensions = new()
        { "abap", "ada", "c", "cbl", "cls", "cob", "cpp", "cs", "css", "dart", "f", "f90", "for", "go", "gml", "groovy",
                "java", "jl", "js", "json", "jsp", "jspx", "kt", "kts", "lua", "m", "mat", "pas", "php", "php3", "pl", "pp", "py", "r",
                "rb", "rs", "sc", "scala", "swift", "ts", "yaml", "bmp", "pam", "pbm", "ppm", "xbm", "xpm", "asp", "aspx", "dhtml", "htm",
                "html", "htmls", "rhtml", "sgml", "xaml", "xhtml", "xml", "pdf", "xlr", "xls", "xlsb", "xlsm", "xlsx", "xlt", "xltm", "xltx",
                "xlw", "bat", "btm", "cmd", "com", "command", "dll", "doc", "docm", "docx", "dotm", "dotx", "ini", "odt", "rtf", "txt", "xps" };

    /// <summary>
    /// Checks if the file extension should be searched with hash.
    /// </summary>
    /// <param name="extension"></param>
    /// <returns>True if the extension should be hash searched</returns>
    public static bool ShouldHashSearch(string extension)
    {
        if (extension.Length > 0)
        {
            extension = extension[1..];

            if (HashSearchableExtensions.Contains(extension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public static ItemType GetFileType(string extension)
    {
        // Remove the first dot in the extension name if any
        // Else, return the file type as File
        if (extension.Length > 0)
        {
            extension = extension[1..];
        }
        else
        {
            return ItemType.File;
        }

        return extension switch
        {
            string x when Android.Contains(x.ToLower()) => ItemType.Android,
            string x when Archive.Contains(x.ToLower()) => ItemType.Archive,
            string x when Audio.Contains(x.ToLower()) => ItemType.Audio,
            string x when Code.Contains(x.ToLower()) => ItemType.Code,
            string x when DiskImage.Contains(x.ToLower()) => ItemType.DiskImage,
            string x when Executable.Contains(x.ToLower()) => ItemType.Executable,
            string x when Font.Contains(x.ToLower()) => ItemType.Font,
            string x when Image.Contains(x.ToLower()) => ItemType.Image,
            string x when MarkupLanguage.Contains(x.ToLower()) => ItemType.MarkupLanguage,
            string x when PDF.Contains(x.ToLower()) => ItemType.PDF,
            string x when Presentation.Contains(x.ToLower()) => ItemType.Presentation,
            string x when Spreadsheet.Contains(x.ToLower()) => ItemType.Spreadsheet,
            string x when System.Contains(x.ToLower()) => ItemType.System,
            string x when Text.Contains(x.ToLower()) => ItemType.Text,
            string x when Video.Contains(x.ToLower()) => ItemType.Video,
            _ => ItemType.File
        };

        //if (Android.Contains(extension.ToLower()))
        //{
        //    return ItemType.Android;
        //}
        //else if (Archive.Contains(extension.ToLower()))
        //{
        //    return ItemType.Archive;
        //}
        //else if (Audio.Contains(extension.ToLower()))
        //{
        //    return ItemType.Audio;
        //}
        //else if (Code.Contains(extension.ToLower()))
        //{
        //    return ItemType.Code;
        //}
        //else if (DiskImage.Contains(extension.ToLower()))
        //{
        //    return ItemType.DiskImage;
        //}
        //else if (Executable.Contains(extension.ToLower()))
        //{
        //    return ItemType.Executable;
        //}
        //else if (Font.Contains(extension.ToLower()))
        //{
        //    return ItemType.Font;
        //}
        //else if (Image.Contains(extension.ToLower()))
        //{
        //    return ItemType.Image;
        //}
        //else if (MarkupLanguage.Contains(extension.ToLower()))
        //{
        //    return ItemType.MarkupLanguage;
        //}
        //else if (PDF.Contains(extension.ToLower()))
        //{
        //    return ItemType.PDF;
        //}
        //else if (Presentation.Contains(extension.ToLower()))
        //{
        //    return ItemType.Presentation;
        //}
        //else if (Spreadsheet.Contains(extension.ToLower()))
        //{
        //    return ItemType.Spreadsheet;
        //}
        //else if (System.Contains(extension.ToLower()))
        //{
        //    return ItemType.System;
        //}
        //else if (Text.Contains(extension.ToLower()))
        //{
        //    return ItemType.Text;
        //}
        //else if (Video.Contains(extension.ToLower()))
        //{
        //    return ItemType.Video;
        //}
        //else
        //{
        //    return ItemType.File;
        //}
    }
}
