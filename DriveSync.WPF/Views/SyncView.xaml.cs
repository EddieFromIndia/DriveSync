namespace DriveSync.Views;

/// <summary>
/// Interaction logic for SyncBackupWindow.xaml
/// </summary>
public partial class SyncView : UserControl
{
    private readonly SyncViewModel syncViewModel;
    public SyncView()
    {
        InitializeComponent();

        syncViewModel = new();
        DataContext = syncViewModel;
    }

    private void ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (syncViewModel.IsLinked)
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
