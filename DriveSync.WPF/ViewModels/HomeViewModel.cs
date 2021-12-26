
namespace DriveSync.ViewModels;

public class HomeViewModel : BaseViewModel
{
    #region Private Properties
    private readonly Window homeWindow;
    private int outerMargin = 10;
    private int windowRadius = 8;
    #endregion

    #region Public Properties
    public int OuterMargin {
        get => homeWindow.WindowState == WindowState.Maximized ? 0 : outerMargin;
        set => outerMargin = value;
    }
    public Thickness OuterMarginThickness => new(OuterMargin);
    public int WindowRadius {
        get => homeWindow.WindowState == WindowState.Maximized ? 0 : windowRadius;
        set => windowRadius = value;
    }
    public CornerRadius WindowCornerRadius => new(WindowRadius);

    public BaseViewModel SelectedViewModel { get; set; }
    #endregion

    #region Command Declarations
    public ICommand CloseButton_Click { get; set; }
    public ICommand MaximizeButton_Click { get; set; }
    public ICommand MinimizeButton_Click { get; set; }
    public ICommand OptionsButton_Click { get; set; }
    public ICommand Header_Click { get; set; }
    public ICommand Header_DoubleClick { get; set; }
    public ICommand Header_RightClick { get; set; }

    public ICommand Settings_Click { get; set; }
    public ICommand Theme_Click { get; set; }
    public ICommand Update_Click { get; set; }
    public ICommand About_Click { get; set; }
    public ICommand Help_Click { get; set; }
    public ICommand Report_Click { get; set; }
    public ICommand Contact_Click { get; set; }
    #endregion

    #region Constructor
    public HomeViewModel(Window window)
    {
        homeWindow = window;

        ViewModelService.Home = this;
        SelectedViewModel = new MainViewModel();

        // Window State changed event handler
        homeWindow.StateChanged += (sender, e) =>
        {
            OnPropertyChanged(nameof(OuterMargin));
            OnPropertyChanged(nameof(OuterMarginThickness));
            OnPropertyChanged(nameof(WindowRadius));
            OnPropertyChanged(nameof(WindowCornerRadius));
        };

        // Command defintions
        Settings_Click = new Command(Settings);
        Theme_Click = new Command(Settings);
        Update_Click = new Command(CheckForUpdates);
        About_Click = new Command(CheckForUpdates);
        Help_Click = new Command(Help);
        Report_Click = new Command(Help);
        Contact_Click = new Command(Help);

        CloseButton_Click = new Command(CloseWindow);
        MaximizeButton_Click = new Command(MaximizeWindow);
        MinimizeButton_Click = new Command(MinimizeWindow);
        OptionsButton_Click = new Command(Options);
        Header_Click = new Command(MoveWindow);
        Header_DoubleClick = new Command(HeaderDoubleClick);
        Header_RightClick = new Command(ShowSystemMenu);
    }

    #endregion

    #region Navigation Methods
    public void Backup()
    {
        SelectedViewModel = new BackupViewModel();
    }

    public void BackupProcessing(ObservableCollection<BackupJobModel> jobs)
    {
        SelectedViewModel = new BackupProcessingViewModel(jobs);
    }

    public void Sync()
    {
        SelectedViewModel = new SyncViewModel();
    }

    public void Restore()
    {
        SelectedViewModel = new RestoreViewModel();
    }

    public void BackToHome()
    {
        SelectedViewModel = new MainViewModel();
    }
    #endregion

    #region Command Implementations
    private void Settings(object sender)
    {

    }

    private void CheckForUpdates(object sender)
    {

    }

    private void Help(object sender)
    {

    }

    /// <summary>
    /// Closes the window
    /// </summary>
    /// <param name="window"></param>
    private void CloseWindow(object window)
    {
        switch (OperationService.OperationType)
        {
            case OperationType.None:
                ((Window)window).Close();
                break;
            case OperationType.Backup:
                if (DialogService.ShowDialog("Warning", "A backup operation is running. Closing the application may result in the loss of data. Are you sure you want to close the application?", DialogButtonGroup.YesNoCancel, DialogImage.Warning) == DialogResult.Yes)
                {
                    ((Window)window).Close();
                }
                break;
            case OperationType.Restore:
                if (DialogService.ShowDialog("Warning", "A restore operation is running. Closing the application may result in the loss of data. Are you sure you want to close the application?", DialogButtonGroup.YesNoCancel, DialogImage.Warning) == DialogResult.Yes)
                {
                    ((Window)window).Close();
                }
                break;
            case OperationType.Sync:
                if (DialogService.ShowDialog("Warning", "A sync operation is running. Closing the application may result in the loss of data. Are you sure you want to close the application?", DialogButtonGroup.YesNoCancel, DialogImage.Warning) == DialogResult.Yes)
                {
                    ((Window)window).Close();
                }
                break;
            case OperationType.Delete:
                if (DialogService.ShowDialog("Warning", "A delete operation is running. Closing the application may result in the loss of data. Are you sure you want to close the application?", DialogButtonGroup.YesNoCancel, DialogImage.Warning) == DialogResult.Yes)
                {
                    ((Window)window).Close();
                }
                break;
        }
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

    private void Options(object sender)
    {
        ContextMenu cm = new()
        {
            //PlacementTarget = sender as Button,
            UseLayoutRounding = true
        };

        MenuItem mi = new() { Header = "Settings", Icon = Geometry.Parse("M23.267037,22.302983C22.732036,22.302983 22.297036,22.737983 22.297036,23.272984 22.297036,23.806985 22.732036,24.241985 23.267037,24.241985 23.802037,24.241985 24.237038,23.806985 24.237038,23.272984 24.237038,22.737983 23.802037,22.302983 23.267037,22.302983z M23.267037,20.363981C24.871038,20.363981 26.17604,21.668982 26.17604,23.272984 26.17604,24.876986 24.871038,26.181988 23.267037,26.181988 21.663035,26.181988 20.358033,24.876986 20.358033,23.272984 20.358033,21.668982 21.663035,20.363981 23.267037,20.363981z M23.266055,18.424977C20.593019,18.424977 18.417052,20.599993 18.417053,23.272964 18.417052,25.946974 20.593019,28.12196 23.266055,28.12196 25.939032,28.12196 28.11402,25.946974 28.11402,23.272964 28.11402,20.599993 25.939032,18.424977 23.266055,18.424977z M22.307014,14.546004L24.237984,14.546004 24.237984,16.55666 24.298206,16.564337C25.172138,16.698268,25.991792,16.999525,26.723291,17.434257L26.85271,17.514188 28.118011,15.983994 29.596992,17.244991 28.338181,18.768154 28.474275,18.925393C29.017497,19.574835,29.441769,20.32698,29.713214,21.14798L29.736273,21.223372 31.659023,20.878988 31.994025,22.809999 30.051378,23.157947 30.054025,23.272964C30.054025,24.157543,29.883904,25.003137,29.574682,25.778769L29.518977,25.911924 31.271982,26.939002 30.305993,28.637999 28.505569,27.58242 28.409766,27.697393C27.873262,28.320218,27.225853,28.844919,26.498554,29.240516L26.407042,29.287452 27.139017,31.328985 25.323997,31.999986 24.582439,29.931726 24.298206,29.982629C23.961564,30.034221 23.616868,30.060984 23.266055,30.060984 22.87139,30.060984 22.484468,30.027112 22.108044,29.962122L21.934875,29.929698 21.194018,31.996002 19.378999,31.325002 20.112203,29.28004 19.997544,29.220795C19.242944,28.804415,18.575526,28.248597,18.030315,27.588321L18.016926,27.571294 16.217992,28.62598 15.251985,26.927984 17.007041,25.899023 16.890696,25.604546C16.623782,24.877131,16.478025,24.091748,16.478025,23.272964L16.480999,23.143706 14.538998,22.795998 14.875002,20.864987 16.799945,21.209762 16.818842,21.14798C17.103124,20.288169,17.555026,19.503876,18.135634,18.833983L18.202798,18.759785 16.944029,17.235997 18.424011,15.976 19.689524,17.507273 19.705656,17.496802C20.462936,17.028381,21.318656,16.7046,22.233903,16.564337L22.307014,16.555018z"), Command = Settings_Click };
        _ = cm.Items.Add(mi);
        _ = cm.Items.Add(new MenuItem { Header = "Theme", Icon = new Image { Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Assets/settings.png") as ImageSource }, Command = Theme_Click });
        _ = cm.Items.Add(new Separator());
        _ = cm.Items.Add(new MenuItem { Header = "Check for Updates", Icon = new Image { Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Assets/settings.png") as ImageSource }, Command = Update_Click });
        _ = cm.Items.Add(new MenuItem { Header = "About", Icon = new Image { Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Assets/settings.png") as ImageSource }, Command = About_Click });
        _ = cm.Items.Add(new MenuItem { Header = "Help", Icon = new Image { Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Assets/settings.png") as ImageSource }, Command = Help_Click });
        _ = cm.Items.Add(new MenuItem { Header = "Report a Problem", Icon = new Image { Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Assets/settings.png") as ImageSource }, Command = Report_Click });
        _ = cm.Items.Add(new MenuItem { Header = "Contact us", Icon = new Image { Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Assets/settings.png") as ImageSource }, Command = Contact_Click });

        cm.IsOpen = true;
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
