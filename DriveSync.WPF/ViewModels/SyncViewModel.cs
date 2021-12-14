namespace DriveSync.ViewModels;

public class SyncViewModel : BaseViewModel
{
    #region Private Properties
    private static readonly string textIntroMessage = "Choose Original and Backup directories...";
    private ObservableCollection<PathItem> OriginalDirectories = new();
    private ObservableCollection<PathItem> BackupDirectories = new();

    private bool isResolving = false;
    #endregion

    #region Public Properties
    public ObservableCollection<PathItem> OriginalDirectoriesToDisplay { get; set; } = new();
    public ObservableCollection<PathItem> BackupDirectoriesToDisplay { get; set; } = new();
    public string OriginalDisplayText { get; set; } = textIntroMessage;
    public string BackupDisplayText { get; set; } = string.Empty;
    public string OriginalPath { get; set; }
    public string BackupPath { get; set; }
    public string LastOriginalPath { get; set; } = string.Empty;
    public string LastBackupPath { get; set; } = string.Empty;
    public ResolveMethods ResolveMethod { get; set; } = 0;
    public bool ShowEqualEntries { get; set; } = true;
    public bool ShowEmptyFolder { get; set; } = true;
    public bool IsLinked { get; set; } = true;
    public Visibility ProgressVisibility { get; set; } = Visibility.Hidden;
    public string ProgressString { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; } = 0;
    #endregion

    #region Command Declarations
    public ICommand HomeButton_Click { get; set; }
    public ICommand BrowseButton_Click { get; set; }
    public ICommand ScanButton_Click { get; set; }
    public ICommand AutoResolveButton_Click { get; set; }
    public ICommand MergeOriginal_Click { get; set; }
    public ICommand MergeBackup_Click { get; set; }
    public ICommand DeleteButton_Click { get; set; }
    public ICommand ExpandOriginalDirectory { get; set; }
    public ICommand ExpandBackupDirectory { get; set; }
    public ICommand ClearButton_Click { get; set; }
    public ICommand BackButton_Click { get; set; }
    public ICommand RefreshButton_Click { get; set; }
    public ICommand VisibilityButton_Click { get; set; }
    public ICommand ShowHideEmptyFolderButton_Click { get; set; }
    public ICommand SettingsButton_Click { get; set; }
    public ICommand UpdateButton_Click { get; set; }
    public ICommand TutorialButton_Click { get; set; }
    public ICommand Link_Click { get; set; }
    #endregion

    #region Constructor
    public SyncViewModel()
    {
        // Command defintions
        HomeButton_Click = new Command(Home);
        BrowseButton_Click = new Command(Browse);
        ScanButton_Click = new Command(Scan, CanScanOrResolve);
        AutoResolveButton_Click = new Command(AutoResolve, CanScanOrResolve);
        MergeOriginal_Click = new Command(MergeOriginalAsync);
        MergeBackup_Click = new Command(MergeBackupAsync);
        DeleteButton_Click = new Command(Delete);
        ExpandOriginalDirectory = new Command(ExpandOriginal);
        ExpandBackupDirectory = new Command(ExpandBackup);
        ClearButton_Click = new Command(Clear, CanClear);
        BackButton_Click = new Command(Back, CanBack);
        RefreshButton_Click = new Command(Refresh, CanRefresh);
        VisibilityButton_Click = new Command(ToggleVisibility);
        ShowHideEmptyFolderButton_Click = new Command(ShowHideEmptyFolder);
        SettingsButton_Click = new Command(Settings, CanPerformMenuTasks);
        UpdateButton_Click = new Command(CheckForUpdates, CanPerformMenuTasks);
        TutorialButton_Click = new Command(Tutorial);
        Link_Click = new Command(ToggleLink);
    }

    #endregion

    #region Command Implementations
    private void Home(object sender)
    {
        ViewModelService.Home.BackToHome();
    }

    private void Browse(object sender)
    {
        string TempPath = string.Empty;
        switch (sender.ToString())
        {
            case "Original":
                TempPath = OriginalPath;
                break;
            case "Backup":
                TempPath = BackupPath;
                break;
        }

        using System.Windows.Forms.FolderBrowserDialog dialog = new() { ShowNewFolderButton = true, SelectedPath = TempPath };
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();

        if (dialog.SelectedPath != string.Empty)
        {
            if (dialog.SelectedPath != new DirectoryInfo(dialog.SelectedPath).Root.ToString())
            {
                switch (sender.ToString())
                {
                    case "Original":
                        OriginalPath = dialog.SelectedPath;
                        break;
                    case "Backup":
                        BackupPath = dialog.SelectedPath;
                        break;
                }
            }
            else
            {
                if (DialogService.ShowDialog("Access Denied", "You are not authorized to access the root drive. Choose another folder.", DialogButtonGroup.OK, DialogImage.Forbidden) == DialogResult.OK)
                {
                    Browse(sender);
                }
            }
        }
    }

    private bool CanScanOrResolve(object sender)
    {
        if (isResolving)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(OriginalPath) && !string.IsNullOrWhiteSpace(BackupPath))
        {
            if (Directory.Exists(OriginalPath) && Directory.Exists(BackupPath) && OriginalPath != BackupPath)
            {
                return true;
            }
        }

        return false;
    }

    private async void Scan(object sender)
    {
        if (isResolving)
        {
            return;
        }

        LastOriginalPath = OriginalPath;
        LastBackupPath = BackupPath;

        await ScanAsync(OriginalPath, BackupPath);
    }

    private async void AutoResolve(object sender)
    {
        isResolving = true;
        OriginalDisplayText = "Resolving. Please wait!";
        BackupDisplayText = "Resolving. Please wait!";
        ProgressVisibility = Visibility.Visible;
        ProgressString = "Modifying files";
        ProgressPercentage = 10;

        OperationService.OperationType = OperationType.Sync;

        try
        {
            switch (ResolveMethod)
            {
                case ResolveMethods.LeftToRight:
                    await MergeAsync(OriginalPath, BackupPath);
                    break;

                case ResolveMethods.RightToLeft:
                    await MergeAsync(BackupPath, OriginalPath);
                    break;
            }
        }
        catch (UnauthorizedAccessException)
        {
            _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
            OriginalDisplayText = string.Empty;
            BackupDisplayText = string.Empty;
            ProgressVisibility = Visibility.Hidden;
            isResolving = false;
            return;
        }
        catch (PathTooLongException)
        {
            _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the folder names.", DialogButtonGroup.OK, DialogImage.Error);
            OriginalDisplayText = string.Empty;
            BackupDisplayText = string.Empty;
            ProgressVisibility = Visibility.Hidden;
            isResolving = false;
            return;
        }
        catch (DirectoryNotFoundException)
        {
            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            OriginalDisplayText = string.Empty;
            BackupDisplayText = string.Empty;
            ProgressVisibility = Visibility.Hidden;
            isResolving = false;
            return;
        }
        catch (FileNotFoundException)
        {
            _ = DialogService.ShowDialog("File Missing", "A file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            OriginalDisplayText = string.Empty;
            BackupDisplayText = string.Empty;
            ProgressVisibility = Visibility.Hidden;
            isResolving = false;
            return;
        }
        catch (NullReferenceException)
        {
            _ = DialogService.ShowDialog("Folder Missing", "A folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            OriginalDisplayText = string.Empty;
            BackupDisplayText = string.Empty;
            ProgressVisibility = Visibility.Hidden;
            isResolving = false;
            return;
        }
        finally
        {
            OperationService.OperationType = OperationType.None;
        }
        //await ScanAsync(OriginalPath, BackupPath);
        //ProgressString = "Calculating";
        //ProgressPercentage = 20;

        //List<Task> tasks = new List<Task>();
        //switch (ResolveMethod)
        //{
        //    case ResolveMethods.LeftToRightDestructive:
        //    case ResolveMethods.LeftToRightNonDestructive:
        //        foreach (PathItem item in BackupDirectories)
        //        {

        //        }

        //        foreach (PathItem item in OriginalDirectories)
        //        {
        //            if (item.IsFile)
        //            {
        //                tasks.Add(Task.Run(() => File.Copy(item.Item.FullName, item.Item.FullName.Replace(OriginalPath, BackupPath))));
        //            }
        //            else
        //            {
        //                switch (item.Status)
        //                {
        //                    case ItemStatus.ExistsButDifferent:
        //                            tasks.Add(Task.Run(() => MergeAsync(item.Item.FullName, item.Item.FullName.Replace(OriginalPath, BackupPath))));
        //                        break;
        //                    case ItemStatus.DoesNotExist:
        //                        tasks.Add(Task.Run(() => CopyFilesRecursively(item.Item.FullName, BackupPath)));
        //                        break;
        //                }
        //            }
        //        }
        //        break;

        //    case ResolveMethods.RightToLeftDestructive:
        //    case ResolveMethods.RightToLeftNonDestructive:
        //        foreach (PathItem item in BackupDirectories)
        //        {
        //            if (item.IsFile)
        //            {
        //                tasks.Add(Task.Run(() => File.Copy(item.Item.FullName, item.Item.FullName.Replace(BackupPath, OriginalPath))));
        //            }
        //            else
        //            {
        //                switch (item.Status)
        //                {
        //                    case ItemStatus.ExistsButDifferent:
        //                        tasks.Add(Task.Run(() => MergeAsync(item.Item.FullName, item.Item.FullName.Replace(BackupPath, OriginalPath))));
        //                        break;
        //                    case ItemStatus.DoesNotExist:
        //                        tasks.Add(Task.Run(() => CopyFilesRecursively(item.Item.FullName, OriginalPath)));
        //                        break;
        //                }
        //            }
        //        }
        //        break;
        //}


        //await Task.WhenAll(tasks);

        ProgressString = "Verifying";
        ProgressPercentage = 80;

        LastOriginalPath = OriginalPath;
        LastBackupPath = BackupPath;
        await ScanAsync(OriginalPath, BackupPath);

        ProgressString = "Done";
        ProgressPercentage = 100;
        _ = DialogService.ShowDialog("Success", "Resolve Complete.", DialogButtonGroup.OK, DialogImage.Success);
        ProgressString = string.Empty;
        ProgressVisibility = Visibility.Hidden;
        OriginalDisplayText = string.Empty;
        BackupDisplayText = string.Empty;
        isResolving = false;
    }

    private async void MergeOriginalAsync(object sender)
    {
        isResolving = true;
        OperationService.OperationType = OperationType.Sync;
        BackupDisplayText = string.Empty;
        ProgressVisibility = Visibility.Visible;
        ProgressString = "Awaiting authentication";
        ProgressPercentage = 0;
        switch (((PathItem)sender).Status)
        {
            case ItemStatus.ExistsButDifferent:
                if (((PathItem)sender).IsFile)
                {
                    OriginalDisplayText = "Copying file. Please wait!";
                    ProgressString = "Copying";
                    ProgressPercentage = 10;

                    try
                    {
                        await Task.Run(() => File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(OriginalPath, BackupPath), true));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the file name or folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The backup folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "The file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }
                else
                {
                    OriginalDisplayText = "Merging folders. Please wait!";
                    ProgressString = "Merging";
                    ProgressPercentage = 10;

                    try
                    {
                        await MergeAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(OriginalPath, BackupPath));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "A file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "A folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }

                break;

            case ItemStatus.ExistsWithDifferentName:
                if (((PathItem)sender).IsFile)
                {
                    if (!File.Exists(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)))
                    {
                        if (File.Exists(((PathItem)sender).DifferentPath))
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";
                            ProgressString = "Renaming";
                            ProgressPercentage = 10;

                            try
                            {
                                await Task.Run(() => File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (PathTooLongException)
                            {
                                _ = DialogService.ShowDialog("Name Too Long", "The name is too long. This also happens when the file is very deep in the folder structure.", DialogButtonGroup.OK, DialogImage.Error);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (DirectoryNotFoundException)
                            {
                                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (FileNotFoundException)
                            {
                                _ = DialogService.ShowDialog("File Missing", "The file you're trying to rename is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (DialogService.ShowDialog("File Exists", $"The file {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)).Parent.Name} already exists. Do you want to overwrite it?",
                                                     DialogButtonGroup.YesNoCancel, DialogImage.File) == DialogResult.Yes)
                            {
                                OriginalDisplayText = "Renaming file. Please wait!";
                                ProgressString = "Renaming";
                                ProgressPercentage = 10;

                                await Task.Run(() => File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath), true));
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (PathTooLongException)
                        {
                            _ = DialogService.ShowDialog("Name Too Long", "The name is too long. This also happens when the file is very deep in the folder structure.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (DirectoryNotFoundException)
                        {
                            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (FileNotFoundException)
                        {
                            _ = DialogService.ShowDialog("File Missing", "The file you're trying to rename is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (NullReferenceException)
                        {
                            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)))
                    {
                        if (File.Exists(((PathItem)sender).DifferentPath))
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";
                            ProgressString = "Renaming";
                            ProgressPercentage = 10;

                            try
                            {
                                await Task.Run(() => Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (PathTooLongException)
                            {
                                _ = DialogService.ShowDialog("Path Too Long", "The destination path is too long.", DialogButtonGroup.OK, DialogImage.Error);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (DirectoryNotFoundException)
                            {
                                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                OriginalDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (DialogService.ShowDialog("Folder Exists", $"The file {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)).Parent.Name} already exists. Do you want to overwrite it?",
                                                         DialogButtonGroup.YesNoCancel, DialogImage.Folder) == DialogResult.Yes)
                            {
                                OriginalDisplayText = "Renaming folder. Please wait!";
                                ProgressString = "Renaming";
                                ProgressPercentage = 10;

                                await Task.Run(() => Directory.Delete(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath), true));
                                await Task.Run(() => Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)));
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (PathTooLongException)
                        {
                            _ = DialogService.ShowDialog("Path Too Long", "The destination path is too long.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (DirectoryNotFoundException)
                        {
                            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (NullReferenceException)
                        {
                            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            OriginalDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                    }
                }
                break;

            case ItemStatus.DoesNotExist:
                if (((PathItem)sender).IsFile)
                {
                    OriginalDisplayText = "Copying file. Please wait!";
                    ProgressString = "Copying";
                    ProgressPercentage = 10;

                    try
                    {
                        if (!Directory.Exists(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath)))
                        {
                            _ = Directory.CreateDirectory(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath));
                        }

                        await Task.Run(() => File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(OriginalPath, BackupPath), true));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the file name or folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "The file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }
                else
                {
                    OriginalDisplayText = "Copying. Please wait!";
                    ProgressString = "Copying";
                    ProgressPercentage = 10;

                    try
                    {
                        await CopyFilesRecursivelyAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "A file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        OriginalDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }
                break;
        }

        ProgressVisibility = Visibility.Visible;
        ProgressPercentage = 80;
        ProgressString = "Verifying";
        OriginalDisplayText = string.Empty;

        await ScanAsync(LastOriginalPath, LastBackupPath);

        ProgressPercentage = 100;
        ProgressString = "Done";
        _ = DialogService.ShowDialog("Success", "Complete.", DialogButtonGroup.OK, DialogImage.Success);
        ProgressVisibility = Visibility.Hidden;
        isResolving = false;
        OperationService.OperationType = OperationType.None;
    }

    private async void MergeBackupAsync(object sender)
    {
        OriginalDisplayText = string.Empty;
        ProgressVisibility = Visibility.Visible;
        ProgressString = "Awaiting authentication";
        ProgressPercentage = 0;
        OperationService.OperationType = OperationType.Sync;
        switch (((PathItem)sender).Status)
        {
            case ItemStatus.ExistsButDifferent:
                if (((PathItem)sender).IsFile)
                {
                    BackupDisplayText = "Copying file. Please wait!";
                    try
                    {
                        await Task.Run(() => File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(BackupPath, OriginalPath), true));
                    }
                    catch(UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the file name or folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The backup folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "The file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }
                else
                {
                    BackupDisplayText = "Merging folders. Please wait!";
                    ProgressString = "Merging";
                    ProgressPercentage = 10;

                    try
                    {
                        await MergeAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(BackupPath, OriginalPath));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "A file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "A folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }

                break;
            case ItemStatus.ExistsWithDifferentName:
                if (((PathItem)sender).IsFile)
                {
                    if (!File.Exists(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)))
                    {
                        if (File.Exists(((PathItem)sender).DifferentPath))
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";

                            try
                            {
                                await Task.Run(() => File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (PathTooLongException)
                            {
                                _ = DialogService.ShowDialog("Name Too Long", "The name is too long. This also happens when the file is very deep in the folder structure.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (DirectoryNotFoundException)
                            {
                                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (FileNotFoundException)
                            {
                                _ = DialogService.ShowDialog("File Missing", "The file you're trying to rename is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (DialogService.ShowDialog("File Exists", $"The file {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)).Parent.Name} already exists. Do you want to overwrite it?",
                                                     DialogButtonGroup.YesNoCancel, DialogImage.File) == DialogResult.Yes)
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";

                            try
                            {
                                await Task.Run(() => File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath), true));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file.", DialogButtonGroup.OK, DialogImage.Forbidden);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (PathTooLongException)
                            {
                                _ = DialogService.ShowDialog("Name Too Long", "The name is too long. This also happens when the file is very deep in the folder structure.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (DirectoryNotFoundException)
                            {
                                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (FileNotFoundException)
                            {
                                _ = DialogService.ShowDialog("File Missing", "The file you're trying to rename is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)))
                    {
                        if (Directory.Exists(((PathItem)sender).DifferentPath))
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";

                            try
                            {
                                await Task.Run(() => Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (PathTooLongException)
                            {
                                _ = DialogService.ShowDialog("Path Too Long", "The destination path is too long.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                            catch (DirectoryNotFoundException)
                            {
                                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                                BackupDisplayText = string.Empty;
                                ProgressVisibility = Visibility.Hidden;
                                isResolving = false;
                                OperationService.OperationType = OperationType.None;
                                return;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (DialogService.ShowDialog("Folder Exists", $"The file {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)).Parent.Name} already exists. Do you want to overwrite it?",
                                                         DialogButtonGroup.YesNoCancel, DialogImage.Folder) == DialogResult.Yes)
                            {
                                OriginalDisplayText = "Renaming folder. Please wait!";

                                await Task.Run(() => Directory.Delete(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath), true));
                                await Task.Run(() => Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)));
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                            BackupDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (PathTooLongException)
                        {
                            _ = DialogService.ShowDialog("Path Too Long", "The destination path is too long.", DialogButtonGroup.OK, DialogImage.Error);
                            BackupDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (DirectoryNotFoundException)
                        {
                            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            BackupDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                        catch (NullReferenceException)
                        {
                            _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                            BackupDisplayText = string.Empty;
                            ProgressVisibility = Visibility.Hidden;
                            isResolving = false;
                            OperationService.OperationType = OperationType.None;
                            return;
                        }
                    }
                }
                break;
            case ItemStatus.DoesNotExist:
                if (((PathItem)sender).IsFile)
                {
                    BackupDisplayText = "Copying file. Please wait!";

                    try
                    {
                        if (!Directory.Exists(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath)))
                        {
                            _ = Directory.CreateDirectory(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath));
                        }

                        await Task.Run(() => File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(BackupPath, OriginalPath), true));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The destination path is too long.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }
                else
                {
                    BackupDisplayText = "Copying. Please wait!";
                    try
                    {
                        await CopyFilesRecursivelyAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.Parent.ToString().Replace(BackupPath, OriginalPath));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the folder names.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        _ = DialogService.ShowDialog("File Missing", "A file you're trying to copy is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                        BackupDisplayText = string.Empty;
                        ProgressVisibility = Visibility.Hidden;
                        isResolving = false;
                        OperationService.OperationType = OperationType.None;
                        return;
                    }
                }
                break;
        }

        ProgressVisibility = Visibility.Visible;
        ProgressPercentage = 80;
        ProgressString = "Verifying";
        BackupDisplayText = string.Empty;

        await ScanAsync(LastOriginalPath, LastBackupPath);

        ProgressPercentage = 100;
        ProgressString = "Done";
        _ = DialogService.ShowDialog("Success", "Complete.", DialogButtonGroup.OK, DialogImage.Success);
        ProgressVisibility = Visibility.Hidden;
        isResolving = false;
        OperationService.OperationType = OperationType.None;
    }

    private async void Delete(object sender)
    {
        isResolving = true;
        string tempOriginalDisplayText = OriginalDisplayText;
        string tempBackupDisplayText = BackupDisplayText;

        if (Equals(((PathItem)sender).Item.Parent.ToString(), LastOriginalPath))
        {
            OriginalDisplayText = "Deleting. Please wait!";
        }
        else
        {
            BackupDisplayText = "Deleting. Please wait!";
        }

        ProgressVisibility = Visibility.Visible;
        ProgressString = "Awaiting authentication";
        ProgressPercentage = 0;

        if (DialogService.ShowDialog("Delete Confirmation", $"This will delete {((PathItem)sender).Item.Name}! Are you sure?", DialogButtonGroup.YesNo, DialogImage.Warning) == DialogResult.Yes)
        {
            ProgressString = $"Deleting {((PathItem)sender).Item.FullName}";
            ProgressPercentage = 20;

            OperationService.OperationType = OperationType.Delete;

            try
            {
                if (((PathItem)sender).IsFile)
                {
                    await Task.Run(() => File.Delete(((PathItem)sender).Item.FullName));
                }
                else
                {
                    await Task.Run(() => Directory.Delete(((PathItem)sender).Item.FullName, true));
                }
            }
            catch (UnauthorizedAccessException)
            {
                _ = DialogService.ShowDialog("Access Denied", "You are not authorized to access this file/folder.", DialogButtonGroup.OK, DialogImage.Forbidden);
                BackupDisplayText = string.Empty;
                ProgressVisibility = Visibility.Hidden;
                isResolving = false;
                OperationService.OperationType = OperationType.None;
                return;
            }
            catch (PathTooLongException)
            {
                _ = DialogService.ShowDialog("Path Too Long", "The path is too long. Try shortening the file or folder names.", DialogButtonGroup.OK, DialogImage.Error);
                BackupDisplayText = string.Empty;
                ProgressVisibility = Visibility.Hidden;
                isResolving = false;
                OperationService.OperationType = OperationType.None;
                return;
            }
            catch (DirectoryNotFoundException)
            {
                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                BackupDisplayText = string.Empty;
                ProgressVisibility = Visibility.Hidden;
                isResolving = false;
                OperationService.OperationType = OperationType.None;
                return;
            }
            catch (FileNotFoundException)
            {
                _ = DialogService.ShowDialog("File Missing", "A file you're trying to delete is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                BackupDisplayText = string.Empty;
                ProgressVisibility = Visibility.Hidden;
                isResolving = false;
                OperationService.OperationType = OperationType.None;
                return;
            }
            catch (NullReferenceException)
            {
                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
                BackupDisplayText = string.Empty;
                ProgressVisibility = Visibility.Hidden;
                isResolving = false;
                OperationService.OperationType = OperationType.None;
                return;
            }

            ProgressString = "Verifying";
            ProgressPercentage = 80;

            await ScanAsync(LastOriginalPath, LastBackupPath);

            ProgressString = "Done";
            ProgressPercentage = 100;

            _ = DialogService.ShowDialog("Complete", "Deleted successfully.", DialogButtonGroup.OK, DialogImage.Success);
        }
        ProgressString = string.Empty;
        ProgressVisibility = Visibility.Hidden;

        OriginalDisplayText = tempOriginalDisplayText;
        BackupDisplayText = tempBackupDisplayText;
        isResolving = false;
        OperationService.OperationType = OperationType.None;
    }

    private async void ExpandOriginal(object sender)
    {
        if (sender is null || isResolving)
        {
            return;
        }

        if (!((PathItem)sender).IsFile)
        {
            try
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastBackupPath)))
                {
                    await ScanAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastBackupPath));
                }
                else
                {
                    await ScanAsync(((PathItem)sender).Item.FullName, string.Empty);
                }

                LastOriginalPath = ((PathItem)sender).Item.FullName;
                LastBackupPath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(LastOriginalPath).Parent.ToString(), LastBackupPath);
            }
            catch (NullReferenceException)
            {
                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            }
        }
    }

    private async void ExpandBackup(object sender)
    {
        if (sender == null || isResolving)
        {
            return;
        }

        if (!((PathItem)sender).IsFile)
        {
            try
            {
                if (Directory.Exists(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastOriginalPath)))
                {
                    await ScanAsync(((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastOriginalPath), ((PathItem)sender).Item.FullName);
                }
                else
                {
                    await ScanAsync(string.Empty, ((PathItem)sender).Item.FullName);
                }

                LastOriginalPath = ((PathItem)sender).Item.FullName.Replace(new DirectoryInfo(((PathItem)sender).Item.FullName).Parent.ToString(), LastOriginalPath);
                LastBackupPath = ((PathItem)sender).Item.FullName;
            }
            catch (NullReferenceException)
            {
                _ = DialogService.ShowDialog("Folder Missing", "The folder is missing. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            }
        }
    }

    private bool CanClear(object sender)
    {
        if (isResolving)
        {
            return false;
        }

        if (OriginalDisplayText == textIntroMessage
            && string.IsNullOrWhiteSpace(BackupDisplayText)
            && string.IsNullOrWhiteSpace(OriginalPath)
            && string.IsNullOrWhiteSpace(BackupPath)
            && LastOriginalPath == string.Empty
            && LastBackupPath == string.Empty)
        {
            return false;
        }

        return true;
    }

    private void Clear(object sender)
    {
        OriginalDisplayText = textIntroMessage;
        BackupDisplayText = string.Empty;

        OriginalDirectories = new ObservableCollection<PathItem>();
        BackupDirectories = new ObservableCollection<PathItem>();

        OriginalDirectoriesToDisplay = new ObservableCollection<PathItem>();
        BackupDirectoriesToDisplay = new ObservableCollection<PathItem>();

        OriginalPath = string.Empty;
        BackupPath = string.Empty;

        LastOriginalPath = string.Empty;
        LastBackupPath = string.Empty;
    }

    private void ToggleVisibility(object sender)
    {
        ShowEqualEntries = !ShowEqualEntries;

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
        if (isResolving)
        {
            return false;
        }

        return (!string.IsNullOrEmpty(LastOriginalPath) || !string.IsNullOrEmpty(LastBackupPath))
            && string.Concat(LastOriginalPath.AsSpan(0, LastOriginalPath.LastIndexOf("\\")), "\\") != new DirectoryInfo(LastOriginalPath).Root.ToString() &&
            string.Concat(LastBackupPath.AsSpan(0, LastBackupPath.LastIndexOf("\\")), "\\") != new DirectoryInfo(LastBackupPath).Root.ToString();
    }

    private async void Back(object sender)
    {
        LastOriginalPath = LastOriginalPath[..LastOriginalPath.LastIndexOf("\\")];
        LastBackupPath = LastBackupPath[..LastBackupPath.LastIndexOf("\\")];

        await ScanAsync(LastOriginalPath, LastBackupPath);
    }

    private bool CanRefresh(object sender)
    {
        if (isResolving)
        {
            return false;
        }

        return !string.IsNullOrEmpty(LastOriginalPath) && !string.IsNullOrEmpty(LastBackupPath);
    }

    private async void Refresh(object sender)
    {
        await ScanAsync(LastOriginalPath, LastBackupPath);
    }

    private bool CanPerformMenuTasks(object sender)
    {
        return !isResolving;
    }

    private void Settings(object sender)
    {

    }

    private void CheckForUpdates(object sender)
    {

    }

    private void Tutorial(object sender)
    {

    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Scans two paths given for discrepancies.
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="backupPath"></param>
    /// <returns></returns>
    private async Task ScanAsync(string originalPath, string backupPath)
    {
        OriginalDirectories.Clear();
        BackupDirectories.Clear();

        string previousOriginalMessage = OriginalDisplayText;
        string previousBackupMessage = BackupDisplayText;
        OriginalDisplayText = "Scanning...";
        BackupDisplayText = "Scanning...";

        try
        {
            if (!string.IsNullOrEmpty(originalPath) && Directory.Exists(originalPath) && !string.IsNullOrEmpty(backupPath) && Directory.Exists(backupPath))
            {
                // Initializing OriginalDirectories
                foreach (string dir in Directory.GetDirectories(originalPath))
                {
                    OriginalDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, Type = ItemType.Folder, DifferentPath = null });
                }

                foreach (string file in Directory.GetFiles(originalPath))
                {
                    OriginalDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, Type = FileExtensions.GetFileType(new DirectoryInfo(file).Extension), DifferentPath = null });
                }

                // Initializing BackupDirectories
                foreach (string dir in Directory.GetDirectories(backupPath))
                {
                    BackupDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, Type = ItemType.Folder, DifferentPath = null });
                }

                foreach (string file in Directory.GetFiles(backupPath))
                {
                    BackupDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, Type = FileExtensions.GetFileType(new DirectoryInfo(file).Extension), DifferentPath = null });
                }

                foreach (PathItem originalDir in OriginalDirectories)
                {
                    // Checks if the original folder or file exists in backup
                    if ((!originalDir.IsFile && Directory.Exists(originalDir.Item.FullName.Replace(originalDir.Item.Parent.ToString(), backupPath))) || (originalDir.IsFile && File.Exists(originalDir.Item.FullName.Replace(originalDir.Item.Parent.ToString(), backupPath))))
                    {
                        // Checks if original and backup folders or files are equal
                        if (await CompareFileSystemEntriesAsync(originalDir.Item.FullName, originalDir.Item.FullName.Replace(originalDir.Item.Parent.ToString(), backupPath)))
                        {
                            OriginalDirectories[OriginalDirectories.IndexOf(originalDir)].Status = ItemStatus.ExistsAndEqual;

                            foreach (PathItem backupDir in BackupDirectories)
                            {
                                if (backupDir.Item.Name == originalDir.Item.Name)
                                {
                                    BackupDirectories[BackupDirectories.IndexOf(backupDir)].Status = ItemStatus.ExistsAndEqual;
                                }
                            }
                        }
                        else
                        {
                            OriginalDirectories[OriginalDirectories.IndexOf(originalDir)].Status = ItemStatus.ExistsButDifferent;

                            foreach (PathItem backupDir in BackupDirectories)
                            {
                                if (backupDir.Item.Name == originalDir.Item.Name && backupDir.Status != ItemStatus.ExistsWithDifferentName)
                                {
                                    BackupDirectories[BackupDirectories.IndexOf(backupDir)].Status = ItemStatus.ExistsButDifferent;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!originalDir.IsFile)
                        {
                            if (IsDirectoryEmpty(originalDir.Item.FullName))
                            {
                                continue;
                            }
                        }

                        foreach (PathItem backupDir in BackupDirectories.Where(item => item.IsFile == originalDir.IsFile))
                        {
                            if (await CompareFileSystemEntriesAsync(originalDir.Item.FullName, backupDir.Item.FullName))
                            {
                                OriginalDirectories[OriginalDirectories.IndexOf(originalDir)].Status = ItemStatus.ExistsWithDifferentName;
                                BackupDirectories[BackupDirectories.IndexOf(backupDir)].Status = ItemStatus.ExistsWithDifferentName;

                                OriginalDirectories[OriginalDirectories.IndexOf(originalDir)].DifferentPath = backupDir.Item.FullName;
                                BackupDirectories[BackupDirectories.IndexOf(backupDir)].DifferentPath = originalDir.Item.FullName;
                            }
                        }
                    }
                }

                UpdateDataVisibility();

                OriginalDisplayText = OriginalDirectoriesToDisplay.Count == 0 ? "No folders or files..." : string.Empty;
                BackupDisplayText = BackupDirectoriesToDisplay.Count == 0 ? "No folders or files..." : string.Empty;
            }
            else if ((!string.IsNullOrEmpty(originalPath) || Directory.Exists(originalPath)) && (string.IsNullOrEmpty(backupPath) || !Directory.Exists(backupPath)))
            {
                foreach (string dir in Directory.GetDirectories(originalPath))
                {
                    OriginalDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, Type = ItemType.Folder, DifferentPath = null });
                }

                foreach (string file in Directory.GetFiles(originalPath))
                {
                    OriginalDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, Type = FileExtensions.GetFileType(new DirectoryInfo(file).Extension), DifferentPath = null });
                }

                UpdateDataVisibility();

                OriginalDisplayText = OriginalDirectories.Count == 0 ? "No folders or files..." : string.Empty;
                BackupDisplayText = "Does not exist...";
            }
            else if ((string.IsNullOrEmpty(originalPath) || !Directory.Exists(originalPath)) && (!string.IsNullOrEmpty(backupPath) || Directory.Exists(backupPath)))
            {
                foreach (string dir in Directory.GetDirectories(backupPath))
                {
                    BackupDirectories.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, Type = ItemType.Folder, DifferentPath = null });
                }

                foreach (string file in Directory.GetFiles(backupPath))
                {
                    BackupDirectories.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, Type = FileExtensions.GetFileType(new DirectoryInfo(file).Extension), DifferentPath = null });
                }

                UpdateDataVisibility();

                OriginalDisplayText = "Does not exist...";
                BackupDisplayText = BackupDirectories.Count == 0 ? "No folders or files..." : string.Empty;
            }
        }
        catch (Exception)
        {
            _ = DialogService.ShowDialog("Error", "Scanning failed. Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            Clear(null);
        }
    }

    /// <summary>
    /// Compares the two directories according to size.
    /// </summary>
    /// <param name="entry1"></param>
    /// <param name="entry2"></param>
    /// <returns>true if they are equal; otherwise false</returns>
    private async Task<bool> CompareFileSystemEntriesAsync(string entry1, string entry2)
    {
        //List<Task<long>> tasks = new List<Task<long>>
        //{
        //    Task.Run(() => CalculateSize(entry1)),
        //    Task.Run(() => CalculateSize(entry2))
        //};

        //long[] results = await Task.WhenAll(tasks);

        //return results[0] == results[1];
        if (File.Exists(entry1))
        {
            if (FileExtensions.ShouldHashSearch(new DirectoryInfo(entry1).Extension))
            {
                if (!await Task.Run(() => CompareFileHashes(entry1, entry2)))
                {
                    return false;
                }
            }
            else
            {
                if (new FileInfo(entry1).Length != new FileInfo(entry2).Length)
                {
                    return false;
                }
            }
        }
        else
        {
            // Check all files inside entry1
            foreach (string file in Directory.GetFiles(entry1))
            {
                if (File.Exists(file.Replace(entry1, entry2)))
                {
                    if (FileExtensions.ShouldHashSearch(new DirectoryInfo(file).Extension))
                    {
                        if (!await Task.Run(() => CompareFileHashes(file, file.Replace(entry1, entry2))))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (new FileInfo(file).Length != new FileInfo(file.Replace(entry1, entry2)).Length)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            // Check all folders inside entry1
            foreach (string dir in Directory.GetDirectories(entry1))
            {
                // if items does not exist in entry2, return false, else search deeper
                if (Directory.Exists(dir.Replace(entry1, entry2)))
                {
                    if (!await CompareFileSystemEntriesAsync(dir, dir.Replace(entry1, entry2)))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Calculates the total size of the given path in bytes.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>The file/folder size in bytes.</returns>
    private long CalculateSize(string path)
    {
        long size = 0;
        try
        {
            //Checks if the path is valid or not
            if (!Directory.Exists(path))
            {
                if (File.Exists(path))
                {
                    FileInfo finfo = new(path);
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
                            FileInfo finfo = new(file);
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
                    if (DialogService.ShowDialog("Error", "Unable to calculate folder size.", DialogButtonGroup.OK, DialogImage.Error) == DialogResult.OK)
                    {
                        return size;
                    }
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            if (DialogService.ShowDialog("Access Denied", "You are not authorized to access this folder.", DialogButtonGroup.OK, DialogImage.Forbidden) == DialogResult.OK)
            {
                return size;
            }
        }
        return size;
    }

    /// <summary>
    /// Checks if the files are equal, comparing their hash values.
    /// </summary>
    /// <param name="file1"></param>
    /// <param name="file2"></param>
    /// <returns></returns>
    private static bool CompareFileHashes(string file1, string file2)
    {
        using FileStream fs1 = new FileInfo(file1).OpenRead();
        using FileStream fs2 = new FileInfo(file2).OpenRead();

        byte[] firstHash = MD5.Create().ComputeHash(fs1);
        byte[] secondHash = MD5.Create().ComputeHash(fs2);

        for (int i = 0; i < firstHash.Length; i++)
        {
            if (firstHash[i] != secondHash[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Copies the directory original path to the backup path.
    /// It replaces any contents present in the backup path.
    /// It is called when the folder is not present in the backup.
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="backupPath"></param>
    private static async Task CopyFilesRecursivelyAsync(string originalPath, string backupPath)
    {
        // Creates all of the directories
        if (Directory.Exists(originalPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath)))
        {
            await Task.Run(() => Directory.Delete(originalPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath), true));
        }

        _ = Directory.CreateDirectory(originalPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath));
        foreach (string dirPath in Directory.GetDirectories(originalPath, "*", SearchOption.AllDirectories))
        {
            _ = Directory.CreateDirectory(dirPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath));
        }

        // Copies all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(originalPath, "*.*", SearchOption.AllDirectories))
        {
            await Task.Run(() => File.Copy(newPath, newPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath), true));
        }
    }

    /// <summary>
    /// Merges the directory original path to the backup path.
    /// It replaces any contents present in the backup path.
    /// It is called when the folder is present in the backup, but has different contents.
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="backupPath"></param>
    private async Task MergeAsync(string originalPath, string backupPath)
    {
        List<Task> tasks = new();

        // Deletes folders in backup that are not in original
        foreach (string tDir in Directory.GetDirectories(backupPath))
        {
            if (Directory.GetDirectories(originalPath, new DirectoryInfo(tDir).Name).Length == 0)
            {
                tasks.Add(Task.Run(() => Directory.Delete(tDir, true)));
            }
        }

        // Deletes files in backup that are not in original
        foreach (string tFile in Directory.GetFiles(backupPath))
        {
            if (Directory.GetFiles(originalPath, new DirectoryInfo(tFile).Name).Length == 0)
            {
                tasks.Add(Task.Run(() => File.Delete(tFile)));
            }
        }

        await Task.WhenAll(tasks);
        tasks.Clear();

        // For every File in original, if file exists in backup but aren't equal,
        // or if it doesn't exist, copy it to backup
        foreach (string sFile in Directory.GetFiles(originalPath))
        {
            string[] tFiles = Directory.GetFiles(backupPath, new DirectoryInfo(sFile).Name);

            if (tFiles.Length > 0)
            {
                if (!await CompareFileSystemEntriesAsync(sFile, tFiles[0]))
                {
                    tasks.Add(Task.Run(() => File.Copy(sFile, sFile.Replace(originalPath, backupPath), true)));
                }
            }
            else
            {
                tasks.Add(Task.Run(() => File.Copy(sFile, sFile.Replace(originalPath, backupPath), true)));
            }
        }

        await Task.WhenAll(tasks);

        // For every Folder in original, if folder exists in backup but aren't equal,
        // merge it to backup, or if it doesn't exist, copy it to backup
        foreach (string sDir in Directory.GetDirectories(originalPath))
        {
            string[] tDirs = Directory.GetDirectories(backupPath, new DirectoryInfo(sDir).Name);

            if (tDirs.Length > 0)
            {
                if (!await CompareFileSystemEntriesAsync(sDir, tDirs[0]))
                {
                    await MergeAsync(sDir, tDirs[0]);
                }
            }
            else
            {
                await CopyFilesRecursivelyAsync(sDir, backupPath);
            }
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
    private void UpdateDataVisibility()
    {
        if (ShowEqualEntries && ShowEmptyFolder)
        {
            OriginalDirectoriesToDisplay = new ObservableCollection<PathItem>(OriginalDirectories.OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
            BackupDirectoriesToDisplay = new ObservableCollection<PathItem>(BackupDirectories.OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
        }
        else if (ShowEqualEntries && !ShowEmptyFolder)
        {
            OriginalDirectoriesToDisplay = new ObservableCollection<PathItem>(OriginalDirectories.Where(entry => (!entry.IsFile && !IsDirectoryEmpty(entry.Item.FullName)) || entry.IsFile).OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
            BackupDirectoriesToDisplay = new ObservableCollection<PathItem>(BackupDirectories.Where(entry => (!entry.IsFile && !IsDirectoryEmpty(entry.Item.FullName)) || entry.IsFile).OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
        }
        else if (!ShowEqualEntries && ShowEmptyFolder)
        {
            OriginalDirectoriesToDisplay = new ObservableCollection<PathItem>(OriginalDirectories.Where(entry => entry.Status != ItemStatus.ExistsAndEqual).OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
            BackupDirectoriesToDisplay = new ObservableCollection<PathItem>(BackupDirectories.Where(entry => entry.Status != ItemStatus.ExistsAndEqual).OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
        }
        else
        {
            OriginalDirectoriesToDisplay = new ObservableCollection<PathItem>(OriginalDirectories.Where(entry => entry.Status != ItemStatus.ExistsAndEqual && ((!entry.IsFile && !IsDirectoryEmpty(entry.Item.FullName)) || entry.IsFile)).OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
            BackupDirectoriesToDisplay = new ObservableCollection<PathItem>(BackupDirectories.Where(entry => entry.Status != ItemStatus.ExistsAndEqual && ((!entry.IsFile && !IsDirectoryEmpty(entry.Item.FullName)) || entry.IsFile)).OrderBy(i => i.IsFile).ThenBy(i => i.Item.Name));
        }

        OriginalDisplayText = OriginalDirectoriesToDisplay.Count == 0
            ? string.IsNullOrEmpty(OriginalDisplayText) ? "No folders or files..." : OriginalDisplayText
            : string.Empty;

        BackupDisplayText = BackupDirectoriesToDisplay.Count == 0
            ? string.IsNullOrEmpty(BackupDisplayText) ? "No folders or files..." : BackupDisplayText
            : string.Empty;
    }

    /// <summary>
    /// Checks if a directory is empty.
    /// Throws an exception for a file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>true if empty, false if not.</returns>
    private static bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }
    #endregion
}

public enum ResolveMethods
{
    LeftToRight,
    RightToLeft
}
