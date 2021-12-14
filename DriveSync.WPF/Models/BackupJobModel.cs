namespace DriveSync.Models;

public class BackupJobModel
{
    public string Name { get; set; }
    public string Original { get; set; }
    public ObservableCollection<BackupPathModel> Backups { get; set; }
    public bool Compress { get; set; }
}
