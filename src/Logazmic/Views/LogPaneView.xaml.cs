using System;
using System.Windows;
using System.Windows.Controls;

namespace Logazmic.Views
{
    using Logazmic.ViewModels;

    /// <summary>
    /// Interaction logic for LogPaneView.xaml
    /// </summary>
    public partial class LogPaneView : UserControl
    {
        private ScrollViewer scrollViewer;

        public LogPaneView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var pVM = DataContext as LogPaneViewModel;
            if (pVM == null)
                return;

            pVM.SyncWithSelectedItemRequired -= PVMOnSyncWithSelectedItemRequired;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var pVM = DataContext as LogPaneViewModel;
            if (pVM == null)
                return;

            pVM.SyncWithSelectedItemRequired += PVMOnSyncWithSelectedItemRequired;
        }

        private void PVMOnSyncWithSelectedItemRequired(object sender, EventArgs eventArgs)
        {
            if (LogDataGrid.SelectedItem == null)
                return;

            LogDataGrid.ScrollIntoView(LogDataGrid.SelectedItem);
        }
    }
}
