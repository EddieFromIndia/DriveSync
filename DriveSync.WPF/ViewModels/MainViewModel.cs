﻿namespace DriveSync.ViewModels;

public class MainViewModel : BaseViewModel
{
    #region Public Properties
    #endregion

    #region Command Declarations
    public ICommand BackupButton_Click { get; set; }
    public ICommand SyncButton_Click { get; set; }
    public ICommand RestoreButton_Click { get; set; }
    #endregion

    #region Constructor
    public MainViewModel()
    {
        // Command defintions
        BackupButton_Click = new Command(Backup);
        SyncButton_Click = new Command(Sync);
        RestoreButton_Click = new Command(Restore);
    }

    #endregion

    #region Command Implementations
    private void Backup(object sender)
    {
        ViewModelService.Home.Backup();
    }

    private void Sync(object sender)
    {
        ViewModelService.Home.Sync();
    }

    private void Restore(object sender)
    {
        ViewModelService.Home.Restore();
    }
    #endregion
}
