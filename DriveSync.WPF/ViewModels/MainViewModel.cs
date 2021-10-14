﻿using DriveSync.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public int ResolveMethod { get; set; } = 0;
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
            BackButton_Click = new Command(Back);
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
            lastSourcePath = SourcePath;
            lastTargetPath = TargetPath;

            ScanAsync(SourcePath, TargetPath);
        }

        private void AutoResolve(object sender)
        {

        }

        private void ExpandSource(object sender)
        {
            if (!((PathItem)sender).IsFile)
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), lastTargetPath)))
                {
                    ScanAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), lastTargetPath));
                }
                else
                {
                    ScanAsync(((PathItem)sender).Item.FullName, string.Empty);
                }

                lastSourcePath = ((PathItem)sender).Item.FullName;
                lastTargetPath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(lastSourcePath).Parent.ToString(), lastTargetPath);
            }
        }

        private void ExpandTarget(object sender)
        {
            if (!((PathItem)sender).IsFile)
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), lastSourcePath)))
                {
                    ScanAsync(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), lastSourcePath), ((PathItem)sender).Item.FullName);
                }
                else
                {
                    ScanAsync(string.Empty, ((PathItem)sender).Item.FullName);
                }

                lastSourcePath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), lastSourcePath);
                lastTargetPath = ((PathItem)sender).Item.FullName;
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
                && string.IsNullOrWhiteSpace(TargetPath))
            {
                return false;
            }

            return true;
        }

        private void Clear(object sender)
        {
            SourceDisplayText = textIntroMessage;
            TargetDisplayText = string.Empty;

            SourcePath = string.Empty;
            TargetPath = string.Empty;
        }

        private void ToggleLink(object sender)
        {
            IsLinked = !IsLinked;
        }

        private void Back(object sender)
        {
            if (lastSourcePath.Substring(0, lastSourcePath.LastIndexOf("\\")) + "\\" != new DirectoryInfo(lastSourcePath.Substring(0, lastSourcePath.LastIndexOf("\\"))).Root.ToString() &&
                lastTargetPath.Substring(0, lastTargetPath.LastIndexOf("\\")) + "\\" != new DirectoryInfo(lastTargetPath.Substring(0, lastTargetPath.LastIndexOf("\\"))).Root.ToString())
            {
                lastSourcePath = lastSourcePath.Substring(0, lastSourcePath.LastIndexOf("\\"));
                lastTargetPath = lastTargetPath.Substring(0, lastTargetPath.LastIndexOf("\\"));

                ScanAsync(lastSourcePath, lastTargetPath);
            }
        }

        //private void Back(object sender)
        //{
        //    if (Directory.Exists(lastSourcePath))
        //    {
        //        if (new DirectoryInfo(lastSourcePath).Parent != null)
        //        {
        //            if (Directory.Exists(lastTargetPath))
        //            {
        //                if (new DirectoryInfo(lastTargetPath).Parent != null)
        //                {
        //                    ScanAsync(new DirectoryInfo(lastSourcePath).Parent.FullName, new DirectoryInfo(lastTargetPath).Parent.FullName);

        //                    lastSourcePath = new DirectoryInfo(lastSourcePath).Parent.FullName;
        //                    lastTargetPath = new DirectoryInfo(lastTargetPath).Parent.FullName;
        //                }
        //                else
        //                {
        //                    ScanAsync(new DirectoryInfo(lastSourcePath).Parent.FullName, string.Empty);

        //                    lastSourcePath = new DirectoryInfo(lastSourcePath).Parent.FullName;
        //                    lastTargetPath = lastTargetPath.Substring(0, lastTargetPath.LastIndexOf("\\"));
        //                }
        //            }
        //            else
        //            {
        //                if (Directory.Exists(lastTargetPath.Substring(0, lastTargetPath.LastIndexOf("\\"))))
        //                {
        //                    ScanAsync(new DirectoryInfo(lastSourcePath).Parent.FullName, lastTargetPath.Substring(0, lastTargetPath.LastIndexOf("\\")));
        //                }
        //                else
        //                {
        //                    ScanAsync(new DirectoryInfo(lastSourcePath).Parent.FullName, string.Empty);
        //                }
        //            }
        //        }
        //        else if (Directory.Exists(lastTargetPath))
        //        {
        //            if (new DirectoryInfo(lastTargetPath).Parent != null)
        //            {
        //                ScanAsync(string.Empty, new DirectoryInfo(lastTargetPath).Parent.FullName);

        //                lastSourcePath = lastSourcePath.Substring(0, lastSourcePath.LastIndexOf("\\"));
        //                lastTargetPath = new DirectoryInfo(lastTargetPath).Parent.FullName;
        //            }
        //            else
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    else if (Directory.Exists(lastTargetPath))
        //    {
        //        if (new DirectoryInfo(lastTargetPath).Parent != null)
        //        {
        //            ScanAsync(string.Empty, new DirectoryInfo(lastTargetPath).Parent.FullName);

        //            lastSourcePath = lastSourcePath.Substring(0, lastSourcePath.LastIndexOf("\\"));
        //            lastTargetPath = new DirectoryInfo(lastTargetPath).Parent.FullName;
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }
        //}

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

                SourceDisplayText = SourceDirectories.Count == 0 ? "No folders or files..." : string.Empty;
                TargetDisplayText = TargetDirectories.Count == 0 ? "No folders or files..." : string.Empty;
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
        #endregion
    }
}