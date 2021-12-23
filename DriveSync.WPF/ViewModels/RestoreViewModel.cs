namespace DriveSync.ViewModels;

public class RestoreViewModel : BaseViewModel
{
    #region Private Properties
    private static readonly string textIntroMessage = "Choose a folder to display its contents.";
    private bool isRestoring = false;
    #endregion

    #region Public Properties
    public ObservableCollection<PathItem> DirectoriesToDisplay { get; set; } = new();
    public string DisplayText { get; set; } = textIntroMessage;
    public string FromPath { get; set; }
    public string ToPath { get; set; }
    public string LastPath { get; set; } = string.Empty;
    public Visibility ProgressVisibility { get; set; } = Visibility.Hidden;
    public string ProgressString { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; } = 0;
    #endregion

    #region Command Declarations
    public ICommand HomeButton_Click { get; set; }
    public ICommand BrowseButton_Click { get; set; }
    public ICommand RestoreButton_Click { get; set; }
    public ICommand ExpandDirectory { get; set; }
    public ICommand ClearButton_Click { get; set; }
    public ICommand BackButton_Click { get; set; }
    public ICommand RefreshButton_Click { get; set; }
    public ICommand SettingsButton_Click { get; set; }
    public ICommand UpdateButton_Click { get; set; }
    public ICommand TutorialButton_Click { get; set; }
    #endregion

    #region Constructor
    public RestoreViewModel()
    {
        // Command defintions
        HomeButton_Click = new Command(Home);
        BrowseButton_Click = new Command(Browse);
        RestoreButton_Click = new Command(Restore, CanRestore);
        ExpandDirectory = new Command(Expand);
        ClearButton_Click = new Command(Clear, CanClear);
        BackButton_Click = new Command(Back, CanBack);
        RefreshButton_Click = new Command(Refresh, CanRefresh);
        SettingsButton_Click = new Command(Settings, CanPerformMenuTasks);
        UpdateButton_Click = new Command(CheckForUpdates, CanPerformMenuTasks);
        TutorialButton_Click = new Command(Tutorial);
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
            case "From":
                TempPath = FromPath;
                break;
            case "To":
                TempPath = ToPath;
                break;
        }

        using System.Windows.Forms.FolderBrowserDialog dialog = new() { ShowNewFolderButton = true, SelectedPath = TempPath };
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();

        if (dialog.SelectedPath != string.Empty)
        {
            switch (sender.ToString())
            {
                case "From":
                    if (dialog.SelectedPath == ToPath && !string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        if (DialogService.ShowDialog("Note", "The folder is same as the \"to\" folder. Choose another folder.", DialogButtonGroup.OK, DialogImage.Info) == DialogResult.OK)
                        {
                            Browse(sender);
                        }
                    }
                    else
                    {
                        FromPath = dialog.SelectedPath[^1] == '\\' ? dialog.SelectedPath : dialog.SelectedPath + "\\";
                        Scan(FromPath);
                        LastPath = FromPath;
                    }
                    break;
                case "To":
                    if (dialog.SelectedPath == FromPath && !string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        if (DialogService.ShowDialog("Note", "The folder is same as the \"Restore from\" folder. Choose another folder.", DialogButtonGroup.OK, DialogImage.Info) == DialogResult.OK)
                        {
                            Browse(sender);
                        }
                    }
                    else
                    {
                        ToPath = dialog.SelectedPath[^1] == '\\' ? dialog.SelectedPath : dialog.SelectedPath + "\\";
                    }
                    break;
            }
        }
    }

    private bool CanRestore(object sender)
    {
        if (isRestoring)
        {
            return false;
        }

        return !string.IsNullOrEmpty(FromPath)
            && !string.IsNullOrEmpty(ToPath);
    }

    private async void Restore(object sender)
    {
        await CopyFilesRecursivelyAsync(FromPath, ToPath);
    }

    private void Expand(object sender)
    {
        if (sender is null || isRestoring)
        {
            return;
        }

        if (!((PathItem)sender).IsFile)
        {
            Scan(((PathItem)sender).Item.FullName);
            LastPath = ((PathItem)sender).Item.FullName;
        }
    }

    private bool CanClear(object sender)
    {
        if (isRestoring)
        {
            return false;
        }

        if (DisplayText == textIntroMessage
            && string.IsNullOrEmpty(FromPath)
            && string.IsNullOrEmpty(ToPath)
            && LastPath == string.Empty)
        {
            return false;
        }

        return true;
    }

    private void Clear(object sender)
    {
        DisplayText = textIntroMessage;

        DirectoriesToDisplay = new ObservableCollection<PathItem>();

        FromPath = string.Empty;
        ToPath = string.Empty;

        LastPath = string.Empty;
    }

    private bool CanBack(object sender)
    {
        if (isRestoring)
        {
            return false;
        }

        return !string.IsNullOrEmpty(LastPath)
            && new DirectoryInfo(LastPath).Parent is not null;
    }

    private void Back(object sender)
    {
        LastPath = new DirectoryInfo(LastPath).Parent.ToString();

        Scan(LastPath);
    }

    private bool CanRefresh(object sender)
    {
        if (isRestoring)
        {
            return false;
        }

        return !string.IsNullOrEmpty(LastPath);
    }

    private void Refresh(object sender)
    {
        Scan(LastPath);
    }

    private bool CanPerformMenuTasks(object sender)
    {
        return !isRestoring;
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
    /// Generates folder contents for display.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private void Scan(string path)
    {
        DirectoriesToDisplay.Clear();

        try
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                // Initializing OriginalDirectories
                foreach (string dir in Directory.GetDirectories(path))
                {
                    DirectoriesToDisplay.Add(new PathItem { Item = new DirectoryInfo(dir), IsFile = false, Status = ItemStatus.DoesNotExist, Type = ItemType.Folder, DifferentPath = null });
                }

                foreach (string file in Directory.GetFiles(path))
                {
                    DirectoriesToDisplay.Add(new PathItem { Item = new DirectoryInfo(file), IsFile = true, Status = ItemStatus.DoesNotExist, Type = FileExtensions.GetFileType(new DirectoryInfo(file).Extension), DifferentPath = null });
                }

                DisplayText = DirectoriesToDisplay.Count == 0 ? "No folders or files..." : string.Empty;
            }
            else
            {
                DisplayText = "Does not exist...";
            }
        }
        catch (Exception)
        {
            _ = DialogService.ShowDialog("Error", "Check if the drive is still connected.", DialogButtonGroup.OK, DialogImage.Error);
            Clear(null);
        }
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
        foreach (string dirPath in Directory.GetDirectories(originalPath, "*", SearchOption.AllDirectories))
        {
            if (!dirPath.Contains(":\\System Volume Information") && !dirPath.Contains(":\\$RECYCLE.BIN"))
            {
                _ = Directory.CreateDirectory(dirPath.Replace(originalPath, backupPath));
            }
        }

        // Copies all the files & Replaces any files with the same name
        foreach (string filePath in Directory.GetFiles(originalPath, "*.*", SearchOption.AllDirectories))
        {
            if (!filePath.Contains(":\\System Volume Information") && !filePath.Contains(":\\$RECYCLE.BIN") && !filePath.Contains(":\\Autorun.inf"))
            {
                await Task.Run(() => File.Copy(filePath, filePath.Replace(originalPath, backupPath), true));
            }
        }

        _ = DialogService.ShowDialog("Success", "Restoration complete!.", DialogButtonGroup.OK, DialogImage.Success);
    }
    #endregion
}
