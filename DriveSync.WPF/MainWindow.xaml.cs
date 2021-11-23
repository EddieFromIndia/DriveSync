namespace DriveSync;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel mainViewModel;
    public MainWindow()
    {
        InitializeComponent();

        mainViewModel = new MainViewModel(this);
        DataContext = mainViewModel;

        // Fixes window resize issue
        _ = new WindowResizer(this);
    }

    private void ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (mainViewModel.IsLinked)
        {
            if (sender == OriginalList)
            {
                (GetScrollViewer(BackupList) as ScrollViewer).ScrollToVerticalOffset(e.VerticalOffset);
            }
            else if (sender == BackupList)
            {
                (GetScrollViewer(OriginalList) as ScrollViewer).ScrollToVerticalOffset(e.VerticalOffset);
            }
        }
    }

    public static DependencyObject GetScrollViewer(DependencyObject o)
    {
        // Return the DependencyObject if it is a ScrollViewer
        if (o is ScrollViewer)
        { return o; }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(o, i);

            DependencyObject result = GetScrollViewer(child);
            if (result == null)
            {
                continue;
            }
            else
            {
                return result;
            }
        }
        return null;
    }
}
