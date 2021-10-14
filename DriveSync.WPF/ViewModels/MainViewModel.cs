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
        private static readonly string textIntroMessage = "Choose Source and Target directories\nand press Scan or Copy to begin...";
        private string lastSourcePath = string.Empty;
        private string lastTargetPath = string.Empty;
        #endregion

        #region Public Properties
        public ObservableCollection<PathItem> SourceDirectories { get; set; } = new ObservableCollection<PathItem>();
        public ObservableCollection<PathItem> TargetDirectories { get; set; } = new ObservableCollection<PathItem>();
        public string SourceDisplayText { get; set; } = textIntroMessage;
        public string TargetDisplayText { get; set; } = string.Empty;
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string DeletePath { get; set; }
        public bool ResolveNonDestructively { get; set; } = false;
        #endregion

        #region Command Declarations
        public ICommand BrowseButton_Click { get; set; }
        public ICommand ScanButton_Click { get; set; }
        public ICommand AutoResolveButton_Click { get; set; }
        public ICommand ExpandSourceDirectory { get; set; }
        public ICommand ExpandTargetDirectory { get; set; }
        public ICommand CopyButton_Click { get; set; }
        public ICommand ClearButton_Click { get; set; }
        public ICommand DeleteButton_Click { get; set; }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            BrowseButton_Click = new Command(Browse);
            ScanButton_Click = new Command(Scan, CanScanResolveOrCopy);
            AutoResolveButton_Click = new Command(AutoResolve, CanScanResolveOrCopy);
            ExpandSourceDirectory = new Command(ExpandSource, CanScanResolveOrCopy);
            ExpandTargetDirectory = new Command(ExpandTarget, CanScanResolveOrCopy);
            CopyButton_Click = new Command(CopyAsync, CanScanResolveOrCopy);
            ClearButton_Click = new Command(Clear, CanClear);
            DeleteButton_Click = new Command(Delete, CanDelete);
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
                case "Delete":
                    TempPath = DeletePath;
                    break;
            }

            using System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = TempPath };
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result.ToString() != string.Empty)
            {
                switch (sender.ToString())
                {
                    case "Source":
                        SourcePath = dialog.SelectedPath;
                        break;
                    case "Target":
                        TargetPath = dialog.SelectedPath;
                        break;
                    case "Delete":
                        DeletePath = dialog.SelectedPath;
                        break;
                }
            }
        }

        private bool CanScanResolveOrCopy(object sender)
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
            lastSourcePath = SourcePath;
            lastTargetPath = TargetPath;

            ScanAsync(SourcePath, TargetPath);
        }

        private void AutoResolve(object sender)
        {

        }

        private void ExpandSource(object sender)
        {
            if (Directory.Exists(sender.ToString().Replace(new DirectoryInfo(sender.ToString()).Parent.ToString(), lastTargetPath)))
            {
                ScanAsync(sender.ToString(), sender.ToString().Replace(new DirectoryInfo(sender.ToString()).Parent.ToString(), lastTargetPath));
            }
            else
            {
                ScanAsync(sender.ToString(), string.Empty);
            }

            lastSourcePath = sender.ToString();
            lastTargetPath = sender.ToString().Replace(new DirectoryInfo(sender.ToString()).Parent.ToString(), lastTargetPath);
        }

        private void ExpandTarget(object sender)
        {
            if (Directory.Exists(sender.ToString().Replace(new DirectoryInfo(sender.ToString()).Parent.ToString(), lastSourcePath)))
            {
                ScanAsync(sender.ToString().Replace(new DirectoryInfo(sender.ToString()).Parent.ToString(), lastSourcePath), sender.ToString());
            }
            else
            {
                ScanAsync(string.Empty, sender.ToString());
            }

            lastSourcePath = sender.ToString().Replace(new DirectoryInfo(sender.ToString()).Parent.ToString(), lastSourcePath);
            lastTargetPath = sender.ToString();
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
                            SourceDisplayText = "Copying...";

                            string previousTargetMessage = TargetDisplayText;
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
                && string.IsNullOrEmpty(TargetDisplayText)
                && string.IsNullOrWhiteSpace(SourcePath)
                && string.IsNullOrWhiteSpace(TargetPath)
                && string.IsNullOrWhiteSpace(DeletePath))
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

            SourcePath = string.Empty;
            TargetPath = string.Empty;
            DeletePath = string.Empty;

            ResolveNonDestructively = false;
        }

        private bool CanDelete(object sender)
        {
            if (!string.IsNullOrWhiteSpace(DeletePath))
            {
                if (Directory.Exists(DeletePath))
                {
                    return true;
                }
            }

            return false;
        }

        private void Delete(object sender)
        {
            if (MessageBox.Show($"This will delete {DeletePath}! Are you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Directory.Delete(DeletePath, true);

                _ = MessageBox.Show("Deleted successfully.", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region Helper Methods
        private async void ScanAsync(string sourcePath, string targetPath)
        {
            SourceDirectories.Clear();
            TargetDirectories.Clear();

            SourceDisplayText = "Scanning...";
            TargetDisplayText = "Scanning...";

            if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
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

                SourceDisplayText = SourceDirectories.Count == 0 ? "No folders or files..." : string.Empty;
                TargetDisplayText = TargetDirectories.Count == 0 ? "No folders or files..." : string.Empty;
            }
            else if (string.IsNullOrEmpty(targetPath))
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
            else if (string.IsNullOrEmpty(sourcePath))
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
        #endregion
    }
}
