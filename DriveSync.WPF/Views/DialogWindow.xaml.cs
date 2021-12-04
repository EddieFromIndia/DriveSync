namespace DriveSync.Views;
/// <summary>
/// Interaction logic for DialogWindow.xaml
/// </summary>
public partial class DialogWindow : Window
{
    #region Public Properties
    public string Header { get; set; }
    public string Body { get; set; }
    public DialogResult ButtonClicked { get; set; }
    #endregion

    #region Command Declarations
    public ICommand OKCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    public ICommand YesCommand { get; set; }
    public ICommand NoCommand { get; set; }

    public ICommand Header_Click { get; set; }
    public ICommand Header_RightClick { get; set; }
    #endregion

    #region Constructor
    public DialogWindow(string header, string body, DialogButtonGroup button, DialogImage image)
    {
        InitializeComponent();

        Header = header;
        Body = body;
        string dialogImage = image switch
        {
            Enums.DialogImage.Complete => "complete",
            Enums.DialogImage.Info => "info",
            Enums.DialogImage.Error => "error",
            Enums.DialogImage.Forbidden => "forbidden",
            Enums.DialogImage.Warning => "warning",
            Enums.DialogImage.Locked => "locked",
            Enums.DialogImage.File => "file",
            Enums.DialogImage.Folder => "folder",
            Enums.DialogImage.Delete => "delete",
            _ => "info"
        };
        DialogImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/dialog-{dialogImage}.png"));

        OKCommand = new Command(OK);
        CancelCommand = new Command(Cancel);
        YesCommand = new Command(Yes);
        NoCommand = new Command(No);

        Header_Click = new Command(MoveWindow);
        Header_RightClick = new Command(ShowSystemMenu);

        switch (button)
        {
            case DialogButtonGroup.OK:
                ButtonArea.Children.Add(GenerateButton("OK", OKCommand));
                break;
            case DialogButtonGroup.OKCancel:
                ButtonArea.Children.Add(GenerateButton("OK", OKCommand));
                ButtonArea.Children.Add(GenerateButton("Cancel", CancelCommand));
                break;
            case DialogButtonGroup.YesNo:
                ButtonArea.Children.Add(GenerateButton("Yes", YesCommand));
                ButtonArea.Children.Add(GenerateButton("No", NoCommand));
                break;
            case DialogButtonGroup.YesNoCancel:
                ButtonArea.Children.Add(GenerateButton("Yes", YesCommand));
                ButtonArea.Children.Add(GenerateButton("No", NoCommand));
                ButtonArea.Children.Add(GenerateButton("Cancel", CancelCommand));
                break;
        }
    }
    #endregion

    #region Command Implementations
    private void OK(object obj)
    {
        ButtonClicked = Enums.DialogResult.OK;
        DialogResult = true;
    }

    private void Cancel(object obj)
    {
        ButtonClicked = Enums.DialogResult.Cancel;
        DialogResult = true;
    }

    private void Yes(object obj)
    {
        ButtonClicked = Enums.DialogResult.Yes;
        DialogResult = true;
    }

    private void No(object obj)
    {
        ButtonClicked = Enums.DialogResult.No;
        DialogResult = true;
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
    private static Button GenerateButton(string content, ICommand command)
    {
        Button button = new()
        {
            Content = content,
            Style = (Style)Application.Current.FindResource("GeneralButton"),
            Margin = new Thickness(10, 0, 0, 0),
            Padding = new Thickness(20, 0, 20, 0),
            Command = command
        };

        return button;
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
