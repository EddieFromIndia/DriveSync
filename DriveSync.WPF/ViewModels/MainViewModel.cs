using DriveSync.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DriveSync.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Private Properties
        private static readonly string textIntroMessage = "Choose Source and Target directories...";
        #endregion

        #region Public Properties
        public ObservableCollection<PathItem> SourceDirectories { get; set; } = new ObservableCollection<PathItem>();
        public ObservableCollection<PathItem> TargetDirectories { get; set; } = new ObservableCollection<PathItem>();
        public ObservableCollection<PathItem> SourceDirectoriesToDisplay { get; set; } = new ObservableCollection<PathItem>();
        public ObservableCollection<PathItem> TargetDirectoriesToDisplay { get; set; } = new ObservableCollection<PathItem>();
        public string SourceDisplayText { get; set; } = textIntroMessage;
        public string TargetDisplayText { get; set; } = string.Empty;
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string LastSourcePath { get; set; } = string.Empty;
        public string LastTargetPath { get; set; } = string.Empty;
        public ResolveMethods ResolveMethod { get; set; } = 0;
        public bool IsVisible { get; set; } = true;
        public bool ShowEmptyFolder { get; set; } = true;
        public bool IsLinked { get; set; } = true;
        public Visibility ProgressVisibility { get; set; } = Visibility.Hidden;
        public string ProgressString { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; } = 0;
        #endregion

        #region Command Declarations
        public ICommand BrowseButton_Click { get; set; }
        public ICommand ScanButton_Click { get; set; }
        public ICommand AutoResolveButton_Click { get; set; }
        public ICommand ExpandSourceDirectory { get; set; }
        public ICommand ExpandTargetDirectory { get; set; }
        public ICommand ClearButton_Click { get; set; }
        public ICommand BackButton_Click { get; set; }
        public ICommand RefreshButton_Click { get; set; }
        public ICommand VisibilityButton_Click { get; set; }
        public ICommand ShowHideEmptyFolderButton_Click { get; set; }
        public ICommand Link_Click { get; set; }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            BrowseButton_Click = new Command(Browse);
            ScanButton_Click = new Command(Scan, CanScanOrResolve);
            AutoResolveButton_Click = new Command(AutoResolve, CanScanOrResolve);
            ExpandSourceDirectory = new Command(ExpandSource);
            ExpandTargetDirectory = new Command(ExpandTarget);
            ClearButton_Click = new Command(Clear, CanClear);
            BackButton_Click = new Command(Back, CanBack);
            RefreshButton_Click = new Command(Refresh, CanRefresh);
            VisibilityButton_Click = new Command(ToggleVisibility);
            ShowHideEmptyFolderButton_Click = new Command(ShowHideEmptyFolder);
            Link_Click = new Command(ToggleLink);
        }

        #endregion

        #region Command Implementations
        private void Browse(object sender)
        {
            string TempPath = string.Empty;
            switch (sender.ToString())
            {
                case "Source":
                    TempPath = SourcePath;
                    break;
                case "Target":
                    TempPath = TargetPath;
                    break;
            }

            using System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = TempPath };
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (dialog.SelectedPath != string.Empty)
            {
                if (dialog.SelectedPath != new DirectoryInfo(dialog.SelectedPath).Root.ToString())
                {
                    switch (sender.ToString())
                    {
                        case "Source":
                            SourcePath = dialog.SelectedPath;
                            break;
                        case "Target":
                            TargetPath = dialog.SelectedPath;
                            break;
                    }
                }
                else
                {
                    if (MessageBox.Show("You are not authorized to access the root drive. Choose another folder.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        Browse(sender);
                    }
                }
            }
        }

        private bool CanScanOrResolve(object sender)
        {
            if (!string.IsNullOrWhiteSpace(SourcePath) && !string.IsNullOrWhiteSpace(TargetPath))
            {
                if (Directory.Exists(SourcePath) && Directory.Exists(TargetPath))
                {
                    return true;
                }
            }

            return false;
        }

        private async void Scan(object sender)
        {
            LastSourcePath = SourcePath;
            LastTargetPath = TargetPath;

            await ScanAsync(SourcePath, TargetPath);
        }

        private async void AutoResolve(object sender)
        {
            SourceDisplayText = "Resolving. Please wait!";
            TargetDisplayText = "Resolving. Please wait!";
            ProgressVisibility = Visibility.Visible;
            ProgressString = "Modifying files";
            ProgressPercentage = 10;

            switch (ResolveMethod)
            {
                case ResolveMethods.LeftToRightDestructive:
                case ResolveMethods.LeftToRightNonDestructive:
                    await MergeAsync(SourcePath, TargetPath);
                    break;

                case ResolveMethods.RightToLeftDestructive:
                case ResolveMethods.RightToLeftNonDestructive:
                    await MergeAsync(TargetPath, SourcePath);
                    break;
            }

            //await ScanAsync(SourcePath, TargetPath);
            //ProgressString = "Calculating";
            //ProgressPercentage = 20;

            //List<Task> tasks = new List<Task>();
            //switch (ResolveMethod)
            //{
            //    case ResolveMethods.LeftToRightDestructive:
            //    case ResolveMethods.LeftToRightNonDestructive:
            //        foreach (PathItem item in TargetDirectories)
            //        {

            //        }

            //        foreach (PathItem item in SourceDirectories)
            //        {
            //            if (item.IsFile)
            //            {
            //                tasks.Add(Task.Run(() => File.Copy(item.Item.FullName, item.Item.FullName.Replace(SourcePath, TargetPath))));
            //            }
            //            else
            //            {
            //                switch (item.Status)
            //                {
            //                    case ItemStatus.ExistsButDifferent:
            //                            tasks.Add(Task.Run(() => MergeAsync(item.Item.FullName, item.Item.FullName.Replace(SourcePath, TargetPath))));
            //                        break;
            //                    case ItemStatus.DoesNotExist:
            //                        tasks.Add(Task.Run(() => CopyFilesRecursively(item.Item.FullName, TargetPath)));
            //                        break;
            //                }
            //            }
            //        }
            //        break;

            //    case ResolveMethods.RightToLeftDestructive:
            //    case ResolveMethods.RightToLeftNonDestructive:
            //        foreach (PathItem item in TargetDirectories)
            //        {
            //            if (item.IsFile)
            //            {
            //                tasks.Add(Task.Run(() => File.Copy(item.Item.FullName, item.Item.FullName.Replace(TargetPath, SourcePath))));
            //            }
            //            else
            //            {
            //                switch (item.Status)
            //                {
            //                    case ItemStatus.ExistsButDifferent:
            //                        tasks.Add(Task.Run(() => MergeAsync(item.Item.FullName, item.Item.FullName.Replace(TargetPath, SourcePath))));
            //                        break;
            //                    case ItemStatus.DoesNotExist:
            //                        tasks.Add(Task.Run(() => CopyFilesRecursively(item.Item.FullName, SourcePath)));
            //                        break;
            //                }
            //            }
            //        }
            //        break;
            //}

            
            //await Task.WhenAll(tasks);

            ProgressString = "Verifying";
            ProgressPercentage = 80;

            LastSourcePath = SourcePath;
            LastTargetPath = TargetPath;
            await ScanAsync (SourcePath, TargetPath);

            ProgressString = "Done";
            ProgressPercentage = 100;
            _ = MessageBox.Show("Resolve Complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ProgressString = string.Empty;
            ProgressVisibility = Visibility.Hidden;
            SourceDisplayText = string.Empty;
            TargetDisplayText = string.Empty;
        }

        private async void ExpandSource(object sender)
        {
            if (!((PathItem)sender).IsFile)
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastTargetPath)))
                {
                    await ScanAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastTargetPath));
                }
                else
                {
                    await ScanAsync(((PathItem)sender).Item.FullName, string.Empty);
                }

                LastSourcePath = ((PathItem)sender).Item.FullName;
                LastTargetPath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(LastSourcePath).Parent.ToString(), LastTargetPath);
            }
        }

        private async void ExpandTarget(object sender)
        {
            if (!((PathItem)sender).IsFile)
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastSourcePath)))
                {
                    await ScanAsync (((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastSourcePath), ((PathItem)sender).Item.FullName);
                }
                else
                {
                    await ScanAsync (string.Empty, ((PathItem)sender).Item.FullName);
                }

                LastSourcePath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastSourcePath);
                LastTargetPath = ((PathItem)sender).Item.FullName;
            }
        }

        private async void CopyAsync(object sender)
        {
            if (!string.IsNullOrWhiteSpace(SourcePath) && !string.IsNullOrWhiteSpace(TargetPath))
            {
                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(TargetPath))
                    {
                        if (MessageBox.Show("This will overwrite any files present in the target! Are you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            string previousSourceMessage = SourceDisplayText;
                            string previousTargetMessage = TargetDisplayText;
                            SourceDisplayText = "Copying...";
                            TargetDisplayText = "Copying...";

                            await Task.Run(() => CopyFilesRecursively(SourcePath, TargetPath));

                            SourceDisplayText = previousSourceMessage;
                            TargetDisplayText = previousTargetMessage;
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show("Target path does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _ = MessageBox.Show("The source path does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _ = MessageBox.Show("The source and target fields cannot be left blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanClear(object sender)
        {
            if (SourceDisplayText == textIntroMessage
                && string.IsNullOrWhiteSpace(TargetDisplayText)
                && string.IsNullOrWhiteSpace(SourcePath)
                && string.IsNullOrWhiteSpace(TargetPath)
                && LastSourcePath == string.Empty
                && LastTargetPath == string.Empty)
            {
                return false;
            }

            return true;
        }

        private void Clear(object sender)
        {
            SourceDisplayText = textIntroMessage;
            TargetDisplayText = string.Empty;

            SourceDirectories = new ObservableCollection<PathItem>();
            TargetDirectories = new ObservableCollection<PathItem>();
            
            SourceDirectoriesToDisplay = new ObservableCollection<PathItem>();
            TargetDirectoriesToDisplay = new ObservableCollection<PathItem>();

            SourcePath = string.Empty;
            TargetPath = string.Empty;

            LastSourcePath = string.Empty;
            LastTargetPath = string.Empty;
        }

        private void ToggleVisibility(object sender)
        {
            IsVisible = !IsVisible;

            UpdateDataVisibility();
        }

        private void ShowHideEmptyFolder(object sender)
        {
            ShowEmptyFolder = !ShowEmptyFolder;

            UpdateDataVisibility();
        }

        private void ToggleLink(object sender)
        {
            IsLinked = !IsLinked;
        }

        private bool CanBack(object sender)
        {
            return (!string.IsNullOrEmpty(LastSourcePath) || !string.IsNullOrEmpty(LastTargetPath))
                && LastSourcePath.Substring(0, LastSourcePath.LastIndexOf("\\")) + "\\" != new DirectoryInfo(LastSourcePath).Root.ToString() &&
                LastTargetPath.Substring(0, LastTargetPath.LastIndexOf("\\")) + "\\" != new DirectoryInfo(LastTargetPath).Root.ToString();
        }

        private async void Back(object sender)
        {
            LastSourcePath = LastSourcePath.Substring(0, LastSourcePath.LastIndexOf("\\"));
            LastTargetPath = LastTargetPath.Substring(0, LastTargetPath.LastIndexOf("\\"));

            await ScanAsync(LastSourcePath, LastTargetPath);
        }

        private bool CanRefresh(object sender)
        {
            return !string.IsNullOrEmpty(LastSourcePath) && !string.IsNullOrEmpty(LastTargetPath);
        }

        private async void Refresh(object sender)
        {
            await ScanAsync(LastSourcePath, LastTargetPath);
        }

        //private void Delete(object sender)
        //{
        //    if (MessageBox.Show($"This will delete {DeletePath}! Are you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        //    {
        //        Directory.Delete(DeletePath, true);

        //        _ = MessageBox.Show("Deleted successfully.", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}
        #endregion

        #region Helper Methods
        private async Task ScanAsync(string sourcePath, string targetPath)
        {
            SourceDirectories.Clear();
            TargetDirectories.Clear();

            SourceDisplayText = "Scanning...";
            TargetDisplayText = "Scanning...";

            if (!string.IsNullOrEmpty(sourcePath) && Directory.Exists(sourcePath) && !string.IsNullOrEmpty(targetPath) && Directory.Exists(targetPath))
            {
                // Initializing SourceDirectories
                foreach (string dir in Directory.GetDirectories(sourcePath))
                {
                    SourceDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                foreach (string file in Directory.GetFiles(sourcePath))
                {
                    SourceDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                // Initializing TargetDirectories
                foreach (string dir in Directory.GetDirectories(targetPath))
                {
                    TargetDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                foreach (string file in Directory.GetFiles(targetPath))
                {
                    TargetDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                foreach (PathItem sourceDir in SourceDirectories)
                {
                    // Checks if the source directory exists in target
                    if ((!sourceDir.IsFile && Directory.Exists(sourceDir.Item.FullName.Replace(sourceDir.Item.Parent.ToString(), targetPath))) || (sourceDir.IsFile && File.Exists(sourceDir.Item.FullName.Replace(sourceDir.Item.Parent.ToString(), targetPath))))
                    {
                        // Checks if source and target directories are equal
                        if (await CompareFileSystemEntriesAsync(sourceDir.Item.FullName, sourceDir.Item.FullName.Replace(sourceDir.Item.Parent.ToString(), targetPath)))
                        {
                            SourceDirectories[SourceDirectories.IndexOf(sourceDir)].Status = ItemStatus.ExistsAndEqual;

                            foreach (PathItem targetDir in TargetDirectories)
                            {
                                if (targetDir.Item.Name == sourceDir.Item.Name)
                                {
                                    TargetDirectories[TargetDirectories.IndexOf(targetDir)].Status = ItemStatus.ExistsAndEqual;
                                }
                            }
                        }
                        else
                        {
                            SourceDirectories[SourceDirectories.IndexOf(sourceDir)].Status = ItemStatus.ExistsButDifferent;

                            foreach (PathItem targetDir in TargetDirectories)
                            {
                                if (targetDir.Item.Name == sourceDir.Item.Name)
                                {
                                    TargetDirectories[TargetDirectories.IndexOf(targetDir)].Status = ItemStatus.ExistsButDifferent;
                                }
                            }
                        }
                    }
                }

                UpdateDataVisibility();

                SourceDisplayText = SourceDirectoriesToDisplay.Count == 0 ? "No folders or files..." : string.Empty;
                TargetDisplayText = TargetDirectoriesToDisplay.Count == 0 ? "No folders or files..." : string.Empty;
            }
            else if ((!string.IsNullOrEmpty(sourcePath) || Directory.Exists(sourcePath)) && (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath)))
            {
                foreach (string dir in Directory.GetDirectories(sourcePath))
                {
                    SourceDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                foreach (string file in Directory.GetFiles(sourcePath))
                {
                    SourceDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                SourceDisplayText = SourceDirectories.Count == 0 ? "No folders or files..." : string.Empty;
                TargetDisplayText = "Does not exist...";
            }
            else if ((string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath)) && (!string.IsNullOrEmpty(targetPath) || Directory.Exists(targetPath)))
            {
                foreach (string dir in Directory.GetDirectories(targetPath))
                {
                    TargetDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                foreach (string file in Directory.GetFiles(targetPath))
                {
                    TargetDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, RealPath = null });
                }

                SourceDisplayText = "Does not exist...";
                TargetDisplayText = TargetDirectories.Count == 0 ? "No folders or files..." : string.Empty;
            }

            RefreshCollection();
            //_ = MessageBox.Show("Scan Complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Compares the two directories according to size.
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns>true if they are equal; otherwise false</returns>
        private static async Task<bool> CompareFileSystemEntriesAsync(string entry1, string entry2)
        {
            List<Task<long>> tasks = new List<Task<long>>
            {
                Task.Run(() => CalculateSize(entry1)),
                Task.Run(() => CalculateSize(entry2))
            };

            long[] results = await Task.WhenAll(tasks);

            return results[0] == results[1];
        }

        /// <summary>
        /// Calculates the total size of the given path in bytes.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The file/folder size in bytes.</returns>
        private static long CalculateSize(string path)
        {
            long size = 0;
            try
            {
                //Checks if the path is valid or not
                if (!Directory.Exists(path))
                {
                    if (File.Exists(path))
                    {
                        FileInfo finfo = new FileInfo(path);
                        size += finfo.Length;
                    }
                    return size;
                }
                else
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(path))
                        {
                            if (File.Exists(file))
                            {
                                FileInfo finfo = new FileInfo(file);
                                size += finfo.Length;
                            }
                        }

                        foreach (string dir in Directory.GetDirectories(path))
                        {
                            size += CalculateSize(dir);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        if (MessageBox.Show("Unable to calculate folder size.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                        {
                            return size;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (MessageBox.Show("You are not authorized to access this folder.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    return size;
                }
            }
            return size;
        }

        /// <summary>
        /// Copies the directory source path to the target path.
        /// It replaces any contents present in the target path.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            // Creates all of the directories
            if (Directory.Exists(sourcePath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath)))
            {
                Directory.Delete(sourcePath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath), true);
            }

            _ = Directory.CreateDirectory(sourcePath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath));
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                _ = Directory.CreateDirectory(dirPath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath));
            }

            // Copies all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath), true);
            }
        }

        /// <summary>
        /// Merges the directory source path to the target path.
        /// It replaces any contents present in the target path.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        private async Task MergeAsync(string sourcePath, string targetPath)
        {
            List<Task> tasks = new List<Task>();

            // Deletes folders in target that are not in source
            foreach (string tDir in Directory.GetDirectories(targetPath))
            {
                if (Directory.GetDirectories(sourcePath, new DirectoryInfo(tDir).Name).Length == 0)
                {
                    tasks.Add(Task.Run(() => Directory.Delete(tDir, true)));
                }
            }

            // Deletes files in target that are not in source
            foreach (string tFile in Directory.GetFiles(targetPath))
            {
                if (Directory.GetFiles(sourcePath, new DirectoryInfo(tFile).Name).Length == 0)
                {
                    tasks.Add(Task.Run(() => File.Delete(tFile)));
                }
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            // For every File in source, if file exists in target but aren't equal,
            // or if it doesn't exist, copy it to target
            foreach (string sFile in Directory.GetFiles(sourcePath))
            {
                string[] tFiles = Directory.GetFiles(targetPath, new DirectoryInfo(sFile).Name);

                if (tFiles.Length > 0)
                {
                    if (!await CompareFileSystemEntriesAsync(sFile, tFiles[0]))
                    {
                        tasks.Add(Task.Run(() => File.Copy(sFile, sFile.Replace(sourcePath, targetPath), true)));
                    }
                }
                else
                {
                    tasks.Add(Task.Run(() => File.Copy(sFile, sFile.Replace(sourcePath, targetPath), true)));
                }
            }

            await Task.WhenAll(tasks);

            // For every Folder in source, if folder exists in target but aren't equal,
            // merge it to target, or if it doesn't exist, copy it to target
            foreach (string sDir in Directory.GetDirectories(sourcePath))
            {
                string[] tDirs = Directory.GetDirectories(targetPath, new DirectoryInfo(sDir).Name);

                if (tDirs.Length > 0)
                {
                    if (!await CompareFileSystemEntriesAsync(sDir, tDirs[0]))
                    {
                        await MergeAsync(sDir, tDirs[0]);
                    }
                }
                else
                {
                    await Task.Run(() => CopyFilesRecursively(sDir, targetPath));
                }
            }
        }

        /// <summary>
        /// Re-scans the source and target paths.
        /// </summary>
        /// <remarks>
        /// It is necessary when the source or target files/folders are modified
        /// outside the application and needs to be updated in the display.
        /// </remarks>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        private void RefreshCollection()
        {
            ObservableCollection<PathItem> tempList = new ObservableCollection<PathItem>();
            foreach (PathItem item in SourceDirectories)
            {
                tempList.Add(item);
            }
            SourceDirectories.Clear();
            foreach (PathItem item in tempList)
            {
                SourceDirectories.Add(item);
            }

            tempList.Clear();
            foreach (PathItem item in TargetDirectories)
            {
                tempList.Add(item);
            }
            TargetDirectories.Clear();
            foreach (PathItem item in tempList)
            {
                TargetDirectories.Add(item);
            }
        }

        /// <summary>
        /// Updates the displayed files and folders according to the flags set by user.
        /// It automatically gets called while scanning, or when a flag is changed.
        /// </summary>
        /// <remarks>
        /// Flags that it accounts for are:
        /// <list type="bullet">
        /// <item>Equal file and folder visibility</item>
        /// <item>Empty folder visibility</item>
        /// </list>
        /// </remarks>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        private void UpdateDataVisibility()
        {
            if (IsVisible && ShowEmptyFolder)
            {
                SourceDirectoriesToDisplay = new ObservableCollection<PathItem>(SourceDirectories);
                TargetDirectoriesToDisplay = new ObservableCollection<PathItem>(TargetDirectories);
            }
            else
            {
                SourceDirectoriesToDisplay = new ObservableCollection<PathItem>(SourceDirectories);
                TargetDirectoriesToDisplay = new ObservableCollection<PathItem>(TargetDirectories);

                if (SourceDirectories.Count > 0)
                {
                    foreach (PathItem dir in SourceDirectories)
                    {
                        if (!IsVisible && dir.Status == ItemStatus.ExistsAndEqual)
                        {
                            _ = SourceDirectoriesToDisplay.Remove(dir);
                        }

                        if (!ShowEmptyFolder && !dir.IsFile)
                        {
                            if (IsDirectoryEmpty(dir.Item.FullName))
                            {
                                _ = SourceDirectoriesToDisplay.Remove(dir);
                            }
                        }
                    }
                }

                if (TargetDirectories.Count > 0)
                {
                    foreach (PathItem dir in TargetDirectories)
                    {
                        if (!IsVisible && dir.Status == ItemStatus.ExistsAndEqual)
                        {
                            _ = TargetDirectoriesToDisplay.Remove(dir);
                        }

                        if (!ShowEmptyFolder && !dir.IsFile)
                        {
                            if (IsDirectoryEmpty(dir.Item.FullName))
                            {
                                _ = TargetDirectoriesToDisplay.Remove(dir);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a directory is empty.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if empty, false if not.</returns>
        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        #endregion
    }

    public enum ResolveMethods
    {
        LeftToRightDestructive,
        LeftToRightNonDestructive,
        RightToLeftDestructive,
        RightToLeftNonDestructive
    }
}