using DriveSync.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DriveSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel mainViewModel;
        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (mainViewModel.IsLinked)
            {
                if (sender == SourceList)
                {
                    (GetScrollViewer(TargetList) as ScrollViewer).ScrollToVerticalOffset(e.VerticalOffset);
                }
                else if (sender == TargetList)
                {
                    (GetScrollViewer(SourceList) as ScrollViewer).ScrollToVerticalOffset(e.VerticalOffset);
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

        private static string CreateMd5(string path)
        {
            List<string> files = new List<string>();
            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                files = Directory.GetFiles(path, "*", SearchOption.AllDirectories).OrderBy(p => p).ToList();
            }
            else
            {
                files.Add(path);
            }

            MD5 md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];

                // hash path
                string relativePath = file.Substring(path.Length + 1);
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                _ = md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                byte[] contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                {
                    _ = md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                }
                else
                {
                    _ = md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                }
            }

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }
    }
}
