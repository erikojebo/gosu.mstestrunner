using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Gosu.MsTestRunner.UI.ViewModels;
using Microsoft.Win32;

namespace Gosu.MsTestRunner.UI
{
    public partial class MainWindow
    {
        private bool _autoScroll = true;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new TestListViewModel();

            Loaded += MainWindow_Loaded;

            LogScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeTestList();
        }

        internal TestListViewModel ViewModel => DataContext as TestListViewModel;

        private async void OnBrowseConfigFileClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                Title = "Choose config file for the test session"
            };

            var result = fileDialog.ShowDialog();

            if (result == true)
            {
                ViewModel.ConfigFilePath = fileDialog.FileName;
                await ViewModel.InitializeTestList();
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (LogScrollViewer.VerticalOffset == LogScrollViewer.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    _autoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    _autoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (_autoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                LogScrollViewer.ScrollToVerticalOffset(LogScrollViewer.ExtentHeight);
            }
        }
    }
}
