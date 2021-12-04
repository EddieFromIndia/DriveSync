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
    public ICommand Header_Click { get; set; }
    public ICommand Header_DoubleClick { get; set; }
    public ICommand Header_RightClick { get; set; }

    public ICommand SettingsButton_Click { get; set; }
    public ICommand UpdateButton_Click { get; set; }
    public ICommand TutorialButton_Click { get; set; }
    #endregion

    #region Constructor
    public HomeViewModel(Window window)
    {
        homeWindow = window;

        ViewModelService.CurrentViewModel = this;
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
        SettingsButton_Click = new Command(Settings);
        UpdateButton_Click = new Command(CheckForUpdates);
        TutorialButton_Click = new Command(Tutorial);

        CloseButton_Click = new Command(CloseWindow);
        MaximizeButton_Click = new Command(MaximizeWindow);
        MinimizeButton_Click = new Command(MinimizeWindow);
        Header_Click = new Command(MoveWindow);
        Header_DoubleClick = new Command(HeaderDoubleClick);
        Header_RightClick = new Command(ShowSystemMenu);
    }

    #endregion

    #region Command Implementations
    public void Backup()
    {
        SelectedViewModel = new SyncViewModel();
    }

    public void Sync()
    {
        SelectedViewModel = new SyncViewModel();
    }

    public void BackToHome()
    {
        SelectedViewModel = new MainViewModel();
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
