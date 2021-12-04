namespace DriveSync.Views;

/// <summary>
/// Interaction logic for HomeWindow.xaml
/// </summary>
public partial class HomeWindow : Window
{
    public HomeWindow()
    {
        InitializeComponent();

        DataContext = new HomeViewModel(this);

        // Fixes window resize issue
        _ = new WindowResizer(this);
    }
}
