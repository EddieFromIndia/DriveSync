namespace DriveSync.ViewModels;

public class BackupProcessingViewModel : BaseViewModel
{
    #region Private Properties
    private List<BackupJobModel> jobs = new();
    private List<string> jobNames = new();
    private List<string> fromPaths = new();
    private List<string> toPaths = new();

    private bool canceled = false;
    #endregion

    #region Public Properties
    public string JobName { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public int ProgressPercentage { get; set; } = 0;
    #endregion

    #region Command Declarations
    public ICommand StopBackupButton_Click { get; set; }
    #endregion

    #region Constructor
    public BackupProcessingViewModel(ObservableCollection<BackupJobModel> jobs)
    {
        this.jobs = new List<BackupJobModel>(jobs);

        // Command defintion
        StopBackupButton_Click = new Command(StopBackup);

        StartBackup();
    }
    #endregion

    #region Command Implementations
    private void StopBackup(object sender)
    {
        canceled = true;

        JobName = "Stopping backup after this job...";
    }
    #endregion

    #region Helper Methods
    private async void StartBackup()
    {
        List<Task> tasks = new();
        ProgressPercentage = 0;

        foreach (BackupJobModel job in jobs)
        {
            foreach (BackupPathModel backup in job.Backups)
            {
                tasks.Add(CopyFilesRecursivelyAsync(job.Original, backup.Path));

                jobNames.Add(job.Name);
                fromPaths.Add(job.Original);
                toPaths.Add(backup.Path);
            }
        }

        OperationService.OperationType = OperationType.Backup;

        for (int i = 0; i < tasks.Count; i++)
        {
            if (!canceled)
            {
                JobName = jobNames[i];
                From = fromPaths[i];
                To = toPaths[i];

                await tasks[i];

                ProgressPercentage = (i + 1) * 100 / tasks.Count;
            }
            else
            {
                OperationService.OperationType = OperationType.None;
                ViewModelService.Home.Backup();
            }
        }

        _ = DialogService.ShowDialog("Success", "Your data has been backed up successfully.", DialogButtonGroup.OK, DialogImage.Success);
        OperationService.OperationType = OperationType.None;
        ViewModelService.Home.Backup();
    }

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
    }
    #endregion
}
