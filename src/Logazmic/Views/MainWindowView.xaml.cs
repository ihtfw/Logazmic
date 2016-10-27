namespace Logazmic.Views
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using MahApps.Metro.Controls;

    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class MainWindowView
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void MessageFilterTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TryOpenPopUp();
        }

        private void TryOpenPopUp()
        {
            if (MessageFilterItemsControl.Items.OfType<object>().Any())
            {
                MessageFilterPopup.IsOpen = true;
            }
        }

        private void MessageFilterTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TryClosePopUp();
        }

        private void TryClosePopUp()
        {
            if (!MessageFilterItemsControl.Items.OfType<object>().Any())
            {
                MessageFilterPopup.IsOpen = false;
                return;
            }
            if (!MessageFilterPopup.IsFocused && !MessageFilterTextBox.IsFocused)
            {
                MessageFilterPopup.IsOpen = false;
            }
        }

        private void MessageFilterTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MessageFilterTextBox.Text = "";
                TryOpenPopUp();
            }
            else if (e.Key == Key.Escape)
            {
                MessageFilterPopup.IsOpen = false;
            }
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