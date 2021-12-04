namespace DriveSync.ViewModels;

public class MainViewModel : BaseViewModel
{
    #region Public Properties
    #endregion

    #region Command Declarations
    public ICommand BackupButton_Click { get; set; }
    public ICommand SyncButton_Click { get; set; }
    #endregion

    #region Constructor
    public MainViewModel()
    {
        // Command defintions
        BackupButton_Click = new Command(Backup);
        SyncButton_Click = new Command(Sync);
    }

    #endregion

    #region Command Implementations
    private void Backup(object sender)
    {
        ViewModelService.CurrentViewModel.Backup();
    }

    private void Sync(object sender)
    {
        ViewModelService.CurrentViewModel.Sync();
    }
    #endregion
}
