namespace DriveSync.ViewModels;

public class BackupViewModel : BaseViewModel
{
    #region Private Properties
    #endregion

    #region Public Properties
    public ObservableCollection<BackupJobModel> Jobs { get; set; } = new();
    #endregion

    #region Command Declarations
    public ICommand BrowseOriginalButton_Click { get; set; }
    public ICommand BrowseBackupButton_Click { get; set; }
    public ICommand BackupButton_Click { get; set; }
    public ICommand AddJobButton_Click { get; set; }
    public ICommand DeleteJobButton_Click { get; set; }
    public ICommand AddBackupButton_Click { get; set; }
    public ICommand DeleteBackupButton_Click { get; set; }
    public ICommand ClearButton_Click { get; set; }

    public ICommand HomeButton_Click { get; set; }
    public ICommand SettingsButton_Click { get; set; }
    public ICommand UpdateButton_Click { get; set; }
    public ICommand TutorialButton_Click { get; set; }
    #endregion

    #region Constructor
    public BackupViewModel()
    {
        // Command defintions
        BrowseOriginalButton_Click = new Command(BrowseOriginal);
        BrowseBackupButton_Click = new Command(BrowseBackup);
        BackupButton_Click = new Command(Backup, CanBackup);
        AddJobButton_Click = new Command(AddJob);
        DeleteJobButton_Click = new Command(DeleteJob);
        AddBackupButton_Click = new Command(AddBackup);
        DeleteBackupButton_Click = new Command(DeleteBackup);
        ClearButton_Click = new Command(Clear, CanClear);

        HomeButton_Click = new Command(Home);
        SettingsButton_Click = new Command(Settings);
        UpdateButton_Click = new Command(CheckForUpdates);
        TutorialButton_Click = new Command(Tutorial);

        AddJob(null);
    }
    #endregion

    #region Command Implementations
    private void BrowseOriginal(object sender)
    {
        using System.Windows.Forms.FolderBrowserDialog dialog = new() { ShowNewFolderButton = true, SelectedPath = Jobs[Jobs.IndexOf(sender as BackupJobModel)].Original };
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();

        if (dialog.SelectedPath != string.Empty)
        {
            if (dialog.SelectedPath != new DirectoryInfo(dialog.SelectedPath).Root.ToString())
            {
                Jobs[Jobs.IndexOf(sender as BackupJobModel)].Original = dialog.SelectedPath[^1] == '\\' ? dialog.SelectedPath : dialog.SelectedPath + "\\";

                ObservableCollection<BackupJobModel> copy = Jobs;
                Jobs = null;
                Jobs = copy;
            }
            else
            {
                if (DialogService.ShowDialog("Note", "You have chosen the root drive. This will back up all the contents of the drive. Do you wish to continue?", DialogButtonGroup.YesNo, DialogImage.Info) == DialogResult.Yes)
                {
                    Jobs[Jobs.IndexOf(sender as BackupJobModel)].Original = dialog.SelectedPath;

                    ObservableCollection<BackupJobModel> copy = Jobs;
                    Jobs = null;
                    Jobs = copy;
                }
                else
                {
                    BrowseOriginal(sender);
                }
            }
        }
    }

    private void BrowseBackup(object sender)
    {
        (int, int) path = (0, 0);

        for (int i = 0; i < Jobs.Count; i++)
        {
            for (int j = 0; j < Jobs[i].Backups.Count; j++)
            {
                if (sender as BackupPathModel == Jobs[i].Backups[j])
                {
                    path = (i, j);
                }
            }
        }

        using System.Windows.Forms.FolderBrowserDialog dialog = new() { ShowNewFolderButton = true, SelectedPath = Jobs[path.Item1].Backups[path.Item2].Path };
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();

        if (dialog.SelectedPath != string.Empty)
        {
            if (dialog.SelectedPath != new DirectoryInfo(dialog.SelectedPath).Root.ToString())
            {
                Jobs[path.Item1].Backups[path.Item2].Path = dialog.SelectedPath[^1] == '\\' ? dialog.SelectedPath : dialog.SelectedPath + "\\";

                ObservableCollection<BackupJobModel> copy = Jobs;
                Jobs = null;
                Jobs = copy;
            }
            else
            {
                if (DialogService.ShowDialog("Access Denied", "You are not authorized to access the root drive. Choose another folder.", DialogButtonGroup.OK, DialogImage.Forbidden) == DialogResult.OK)
                {
                    BrowseOriginal(sender);
                }
            }
        }
    }

    private bool CanBackup(object sender)
    {
        return Jobs.Count > 0;
    }

    private void Backup(object sender)
    {
        foreach (BackupJobModel job in Jobs)
        {
            if (job.Original is null)
            {
                _ = DialogService.ShowDialog("Empty field warning", "Please fill up all the fields.", DialogButtonGroup.OK, DialogImage.Info);
                return;
            }
            else
            {
                foreach (BackupPathModel backup in job.Backups)
                {
                    if (backup.Path is null)
                    {
                        _ = DialogService.ShowDialog("Empty field warning", "Please fill up all the fields.", DialogButtonGroup.OK, DialogImage.Info);
                        return;
                    }
                }
            }
        }

        ViewModelService.Home.BackupProcessing(Jobs);
    }

    private void AddJob(object sender)
    {
        BackupJobModel newJob = new()
        {
            Name = Jobs.Count > 0 ? $"Job {int.Parse(Jobs[^1].Name[4..]) + 1}" : "Job 1",
            Backups = new(),
            Compress = false
        };
        newJob.Backups.Add(new BackupPathModel());

        Jobs.Add(newJob);
    }

    private void DeleteJob(object sender)
    {
        Jobs.Remove(sender as BackupJobModel);

        if (Jobs.Count == 0)
        {
            AddJob(null);
        }
    }

    private void AddBackup(object sender)
    {
        Jobs[Jobs.IndexOf(sender as BackupJobModel)].Backups.Add(new());
    }

    private void DeleteBackup(object sender)
    {
        (int, int) path = (0, 0);

        for (int i = 0; i < Jobs.Count; i++)
        {
            for (int j = 0; j < Jobs[i].Backups.Count; j++)
            {
                if (sender as BackupPathModel == Jobs[i].Backups[j])
                {
                    path = (i, j);
                }
            }
        }

        Jobs[path.Item1].Backups.RemoveAt(path.Item2);
    }

    private bool CanClear(object sender)
    {
        return Jobs.Count > 0;
    }

    private void Clear(object sender)
    {
        Jobs.Clear();
        AddJob(null);
    }

    private void Home(object sender)
    {
        ViewModelService.Home.BackToHome();
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
}
