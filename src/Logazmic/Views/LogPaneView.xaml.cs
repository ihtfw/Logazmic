using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Logazmic.Utils;

namespace Logazmic.Views
{
    using Caliburn.Micro;

    /// <summary>
    /// Interaction logic for LogPaneView.xaml
    /// </summary>
    public partial class LogPaneView
    {
        private ThrottleHelper throttleHelper;

        public LogPaneView()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            InitializeComponent();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            throttleHelper?.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            throttleHelper?.Dispose();
            throttleHelper = new ThrottleHelper(150, ScrollIntoSelectedInternal);
        }

        public void ScrollIntoSelected()
        {
            throttleHelper.Do();
        }

        private void ScrollIntoSelectedInternal(List<object> objects)
        {
            Execute.OnUIThread(() =>
            {
                if (LogDataGrid.SelectedItem == null)
                    return;
                LogDataGrid.ScrollIntoView(LogDataGrid.SelectedItem);
            });
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }
    }
}
