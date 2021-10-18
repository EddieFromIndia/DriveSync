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
        public int ResolveMethod { get; set; } = 0;
        public bool IsVisible { get; set; } = true;
        public bool ShowEmptyFolder { get; set; } = true;
        public bool IsLinked { get; set; } = true;
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

        private void Scan(object sender)
        {
            LastSourcePath = SourcePath;
            LastTargetPath = TargetPath;

            ScanAsync(SourcePath, TargetPath);
        }

        private void AutoResolve(object sender)
        {

        }

        private void ExpandSource(object sender)
        {
            if (!((PathItem)sender).IsFile)
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastTargetPath)))
                {
                    ScanAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastTargetPath));
                }
                else
                {
                    ScanAsync(((PathItem)sender).Item.FullName, string.Empty);
                }

                LastSourcePath = ((PathItem)sender).Item.FullName;
                LastTargetPath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(LastSourcePath).Parent.ToString(), LastTargetPath);
            }
        }

        private void ExpandTarget(object sender)
        {
            if (!((PathItem)sender).IsFile)
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastSourcePath)))
                {
                    ScanAsync(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastSourcePath), ((PathItem)sender).Item.FullName);
                }
                else
                {
                    ScanAsync(string.Empty, ((PathItem)sender).Item.FullName);
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

        private void Back(object sender)
        {
            LastSourcePath = LastSourcePath.Substring(0, LastSourcePath.LastIndexOf("\\"));
            LastTargetPath = LastTargetPath.Substring(0, LastTargetPath.LastIndexOf("\\"));

            ScanAsync(LastSourcePath, LastTargetPath);
        }

        private bool CanRefresh(object sender)
        {
            return !string.IsNullOrEmpty(LastSourcePath) && !string.IsNullOrEmpty(LastTargetPath);
        }

        private void Refresh(object sender)
        {
            ScanAsync(LastSourcePath, LastTargetPath);
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
        private async void ScanAsync(string sourcePath, string targetPath)
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
                    if (Directory.Exists(sourceDir.Item.FullName.Replace(sourceDir.Item.Parent.ToString(), targetPath)) || File.Exists(sourceDir.Item.FullName.Replace(sourceDir.Item.Parent.ToString(), targetPath)))
                    {
                        // Checks if source and target directories are equal
                        if (await CompareDirectoriesAsync(sourceDir.Item.FullName.Replace(sourceDir.Item.Parent.ToString(), targetPath), sourceDir.Item.FullName))
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
        private static async Task<bool> CompareDirectoriesAsync(string dir1, string dir2)
        {
            List<Task<long>> tasks = new List<Task<long>>();

            tasks.Add(Task.Run(() => CalculateSize(dir1)));
            tasks.Add(Task.Run(() => CalculateSize(dir2)));

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
            long folderSize = 0;
            try
            {
                //Checks if the path is valid or not
                if (!Directory.Exists(path))
                {
                    if (File.Exists(path))
                    {
                        FileInfo finfo = new FileInfo(path);
                        folderSize += finfo.Length;
                    }
                    return folderSize;
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
                                folderSize += finfo.Length;
                            }
                        }

                        foreach (string dir in Directory.GetDirectories(path))
                        {
                            folderSize += CalculateSize(dir);
                        }
                    }
                    catch (NotSupportedException)
                    {
                        if (MessageBox.Show("Unable to calculate folder size.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                        {
                            return folderSize;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (MessageBox.Show("You are not authorized to access this folder.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    return folderSize;
                }
            }
            return folderSize;
        }

        /// <summary>
        /// Copies the contents of the source path to the target path.
        /// It replaces any contents present in the target path.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Creates all of the directories
            if (Directory.Exists(sourcePath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath)))
            {
                Directory.Delete(sourcePath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath), true);
            }

            _ = Directory.CreateDirectory(sourcePath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath));
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                _ = Directory.CreateDirectory(dirPath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath));
            }

            //Copies all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(new DirectoryInfo(sourcePath).Parent.ToString(), targetPath), true);
            }

            _ = MessageBox.Show("Copy Complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        #endregion
    }
}