using System;

namespace DriveSync.ViewModels;

public class MainViewModel : BaseViewModel
{
    #region Private Properties
    private readonly Window mainWindow;
    private int outerMargin = 10;
    private int windowRadius = 8;

    private static readonly string textIntroMessage = "Choose Original and Backup directories...";
    private ObservableCollection<PathItem> OriginalDirectories = new();
    private ObservableCollection<PathItem> BackupDirectories = new();
    #endregion

    #region Public Properties
    public int OuterMargin {
        get => mainWindow.WindowState == WindowState.Maximized ? 0 : outerMargin;
        set => outerMargin = value;
    }
    public Thickness OuterMarginThickness => new(OuterMargin);
    public int WindowRadius {
        get => mainWindow.WindowState == WindowState.Maximized ? 0 : windowRadius;
        set => windowRadius = value;
    }
    public CornerRadius WindowCornerRadius => new(WindowRadius);

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
    public ICommand CloseButton_Click { get; set; }
    public ICommand MaximizeButton_Click { get; set; }
    public ICommand MinimizeButton_Click { get; set; }
    public ICommand Header_Click { get; set; }
    public ICommand Header_DoubleClick { get; set; }
    public ICommand Header_RightClick { get; set; }

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
    public MainViewModel(Window window)
    {
        mainWindow = window;

        // Window State changed event handler
        mainWindow.StateChanged += (sender, e) => {
            OnPropertyChanged(nameof(OuterMargin));
            OnPropertyChanged(nameof(OuterMarginThickness));
            OnPropertyChanged(nameof(WindowRadius));
            OnPropertyChanged(nameof(WindowCornerRadius));
        };

        // Command defintions
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
        SettingsButton_Click = new Command(Settings);
        UpdateButton_Click = new Command(CheckForUpdates);
        TutorialButton_Click = new Command(Tutorial);
        Link_Click = new Command(ToggleLink);

        CloseButton_Click = new Command(CloseWindow);
        MaximizeButton_Click = new Command(MaximizeWindow);
        MinimizeButton_Click = new Command(MinimizeWindow);
        Header_Click = new Command(MoveWindow);
        Header_DoubleClick = new Command(HeaderDoubleClick);
        Header_RightClick = new Command(ShowSystemMenu);
    }

    #endregion

    #region Command Implementations
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
                if (MessageBox.Show("You are not authorized to access the root drive. Choose another folder.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    Browse(sender);
                }
            }
        }
    }

    private bool CanScanOrResolve(object sender)
    {
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
        LastOriginalPath = OriginalPath;
        LastBackupPath = BackupPath;

        await ScanAsync(OriginalPath, BackupPath);
    }

    private async void AutoResolve(object sender)
    {
        OriginalDisplayText = "Resolving. Please wait!";
        BackupDisplayText = "Resolving. Please wait!";
        ProgressVisibility = Visibility.Visible;
        ProgressString = "Modifying files";
        ProgressPercentage = 10;

        switch (ResolveMethod)
        {
            case ResolveMethods.LeftToRight:
                await MergeAsync(OriginalPath, BackupPath);
                break;

            case ResolveMethods.RightToLeft:
                await MergeAsync(BackupPath, OriginalPath);
                break;
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
        _ = MessageBox.Show("Resolve Complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        ProgressString = string.Empty;
        ProgressVisibility = Visibility.Hidden;
        OriginalDisplayText = string.Empty;
        BackupDisplayText = string.Empty;
    }

    private async void MergeOriginalAsync(object sender)
    {
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

                    File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(OriginalPath, BackupPath), true);
                }
                else
                {
                    OriginalDisplayText = "Merging folders. Please wait!";
                    ProgressString = "Merging";
                    ProgressPercentage = 10;

                    await MergeAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(OriginalPath, BackupPath));
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

                            File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath));
                        }
                    }
                    else
                    {
                        if (MessageBox.Show($"The file {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)).Parent.Name} already exists. Do you want to overwrite it?",
                            "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";
                            ProgressString = "Renaming";
                            ProgressPercentage = 10;

                            File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath), true);
                        }
                        else
                        {
                            ProgressPercentage = 100;
                            ProgressString = "Done";
                            ProgressVisibility = Visibility.Hidden;
                            return;
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

                            Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath));
                        }
                    }
                    else
                    {
                        if (MessageBox.Show($"The folder {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath)).Parent.Name} already exists. Do you want to overwrite it?",
                            "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            OriginalDisplayText = "Renaming folder. Please wait!";
                            ProgressString = "Renaming";
                            ProgressPercentage = 10;

                            Directory.Delete(((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath), true);
                            Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(BackupPath, OriginalPath));
                        }
                        else
                        {
                            ProgressPercentage = 100;
                            ProgressString = "Done";
                            ProgressVisibility = Visibility.Hidden;
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

                    if (!Directory.Exists(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath)))
                    {
                        _ = Directory.CreateDirectory(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath));
                    }

                    File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(OriginalPath, BackupPath), true);
                }
                else
                {
                    OriginalDisplayText = "Copying. Please wait!";
                    ProgressString = "Copying";
                    ProgressPercentage = 10;

                    CopyFilesRecursively(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath));
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
        _ = MessageBox.Show("Done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        ProgressVisibility = Visibility.Hidden;
    }

    private async void MergeBackupAsync(object sender)
    {
        OriginalDisplayText = string.Empty;
        ProgressVisibility = Visibility.Visible;
        ProgressString = "Awaiting authentication";
        ProgressPercentage = 0;
        switch (((PathItem)sender).Status)
        {
            case ItemStatus.ExistsButDifferent:
                if (((PathItem)sender).IsFile)
                {
                    BackupDisplayText = "Copying file. Please wait!";
                    try
                    {
                        File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(BackupPath, OriginalPath), true);
                    }
                    catch(UnauthorizedAccessException)
                    {
                        //Display(You do not have the required permission)
                    }
                    catch (PathTooLongException)
                    {
                        //Display(The path is too long)
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //Display(The backup folder is missing)
                    }
                    catch (FileNotFoundException)
                    {
                        //Display(The original file is missing)
                    }
                }
                else
                {
                    BackupDisplayText = "Merging folders. Please wait!";
                    await MergeAsync(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(BackupPath, OriginalPath));
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
                                File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath));
                            }
                            catch (UnauthorizedAccessException)
                            {
                                //Display(You do not have the required permission)
                            }
                            catch (PathTooLongException)
                            {
                                //Display(The path is too long)
                            }
                            catch (DirectoryNotFoundException)
                            {
                                //Display(The backup folder is missing)
                            }
                            catch (FileNotFoundException)
                            {
                                //Display(The original file is missing)
                            }
                        }
                    }
                    else
                    {
                        if (MessageBox.Show($"The file {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)).Parent.Name} already exists. Do you want to overwrite it?",
                            "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            OriginalDisplayText = "Renaming file. Please wait!";

                            try
                            {
                                File.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath), true);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                //Display(You do not have the required permission)
                            }
                            catch (PathTooLongException)
                            {
                                //Display(The path is too long)
                            }
                            catch (DirectoryNotFoundException)
                            {
                                //Display(The backup folder is missing)
                            }
                            catch (FileNotFoundException)
                            {
                                //Display(The original file is missing)
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

                            Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath));
                        }
                    }
                    else
                    {
                        if (MessageBox.Show($"The folder {new DirectoryInfo(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath)).Parent.Name} already exists. Do you want to overwrite it?",
                            "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            OriginalDisplayText = "Renaming folder. Please wait!";

                            Directory.Delete(((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath), true);
                            Directory.Move(((PathItem)sender).Item.FullName, ((PathItem)sender).DifferentPath.Replace(OriginalPath, BackupPath));
                        }
                    }
                }
                break;
            case ItemStatus.DoesNotExist:
                if (((PathItem)sender).IsFile)
                {
                    BackupDisplayText = "Copying file. Please wait!";

                    if (!Directory.Exists(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath)))
                    {
                        _ = Directory.CreateDirectory(((PathItem)sender).Item.Parent.ToString().Replace(OriginalPath, BackupPath));
                    }

                    File.Copy(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.FullName.Replace(BackupPath, OriginalPath), true);
                }
                else
                {
                    BackupDisplayText = "Copying. Please wait!";
                    CopyFilesRecursively(((PathItem)sender).Item.FullName, ((PathItem)sender).Item.Parent.ToString().Replace(BackupPath, OriginalPath));
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
        _ = MessageBox.Show("Done.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        ProgressVisibility = Visibility.Hidden;
    }

    private async void Delete(object sender)
    {
        if (Equals(((PathItem)sender).Item.Parent.ToString(), LastOriginalPath))
        {
            OriginalDisplayText = "Deleting. Please wait!";
            BackupDisplayText = string.Empty;
        }
        else
        {
            OriginalDisplayText = string.Empty;
            BackupDisplayText = "Deleting. Please wait!";
        }

        ProgressVisibility = Visibility.Visible;
        ProgressString = "Awaiting authentication";
        ProgressPercentage = 0;

        if (MessageBox.Show($"This will delete {((PathItem)sender).Item.Name}! Are you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            ProgressString = $"Deleting {((PathItem)sender).Item.FullName}";
            ProgressPercentage = 20;

            if (((PathItem)sender).IsFile)
            {
                await Task.Run(() => File.Delete(((PathItem)sender).Item.FullName));
            }
            else
            {
                await Task.Run(() => Directory.Delete(((PathItem)sender).Item.FullName, true));
            }

            ProgressString = "Verifying";
            ProgressPercentage = 80;

            await ScanAsync(LastOriginalPath, LastBackupPath);

            ProgressString = "Done";
            ProgressPercentage = 100;

            _ = MessageBox.Show("Deleted successfully.", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        ProgressString = string.Empty;
        ProgressVisibility = Visibility.Hidden;

        OriginalDisplayText = string.Empty;
        BackupDisplayText = string.Empty;
    }

    private async void ExpandOriginal(object sender)
    {
        if (sender == null)
        {
            return;
        }

        if (!((PathItem)sender).IsFile)
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
    }

    private async void ExpandBackup(object sender)
    {
        if (sender == null)
        {
            return;
        }

        if (!((PathItem)sender).IsFile)
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
    }

    private bool CanClear(object sender)
    {
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
        return !string.IsNullOrEmpty(LastOriginalPath) && !string.IsNullOrEmpty(LastBackupPath);
    }

    private async void Refresh(object sender)
    {
        await ScanAsync(LastOriginalPath, LastBackupPath);
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

    /// <summary>
    /// Closes the window
    /// </summary>
    /// <param name="window"></param>
    private void CloseWindow(object window)
    {
        ((Window)window).Close();
    }

    /// <summary>
    /// Maximizes or restores the window
    /// </summary>
    /// <param name="window"></param>
    private void MaximizeWindow(object window)
    {
        ((Window)window).WindowState ^= WindowState.Maximized;
    }

    /// <summary>
    /// Minimizes the window
    /// </summary>
    /// <param name="window"></param>
    private void MinimizeWindow(object window)
    {
        ((Window)window).WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Maximizes or restores the window
    /// </summary>
    /// <param name="window"></param>
    private void HeaderDoubleClick(object window)
    {
        ((Window)window).WindowState ^= WindowState.Maximized;
    }

    /// <summary>
    /// Moves the window
    /// </summary>
    /// <param name="window"></param>
    private void MoveWindow(object window)
    {
        ((Window)window).DragMove();
    }

    /// <summary>
    /// Shows system menu on right click at the header.
    /// </summary>
    /// <param name="window"></param>
    private void ShowSystemMenu(object window)
    {
        SystemCommands.ShowSystemMenu((Window)window, GetMousePosition((Window)window));
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

        OriginalDisplayText = "Scanning...";
        BackupDisplayText = "Scanning...";

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
    /// </summary>
    /// <param name="originalPath"></param>
    /// <param name="backupPath"></param>
    private static void CopyFilesRecursively(string originalPath, string backupPath)
    {
        // Creates all of the directories
        if (Directory.Exists(originalPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath)))
        {
            Directory.Delete(originalPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath), true);
        }

        _ = Directory.CreateDirectory(originalPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath));
        foreach (string dirPath in Directory.GetDirectories(originalPath, "*", SearchOption.AllDirectories))
        {
            _ = Directory.CreateDirectory(dirPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath));
        }

        // Copies all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(originalPath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(new DirectoryInfo(originalPath).Parent.ToString(), backupPath), true);
        }
    }

    /// <summary>
    /// Merges the directory original path to the backup path.
    /// It replaces any contents present in the backup path.
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
                await Task.Run(() => CopyFilesRecursively(sDir, backupPath));
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

    /// <summary>
    /// Gets the mouse position on the screen when the method is called. 
    /// </summary>
    /// <param name="window"></param>
    /// <returns>the mouse position.</returns>
    private static Point GetMousePosition(Window window)
    {
        Point position = Mouse.GetPosition(window);

        return window.WindowState == WindowState.Maximized ? position : new Point(position.X + window.Left, position.Y + window.Top);
    }
    #endregion
}

public enum ResolveMethods
{
    LeftToRight,
    RightToLeft
}
