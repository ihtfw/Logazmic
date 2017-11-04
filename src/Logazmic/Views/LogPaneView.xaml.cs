using System.Windows;
using System.Windows.Controls;

namespace Logazmic.Views
{
    using Caliburn.Micro;

    /// <summary>
    /// Interaction logic for LogPaneView.xaml
    /// </summary>
    public partial class LogPaneView
    {
        public LogPaneView()
        {
            InitializeComponent();
        }

        public void ScrollIntoSelected()
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
