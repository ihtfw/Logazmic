using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Logazmic.Views.Filters
{
    /// <summary>
    /// Interaction logic for ProfilesFiltersView.xaml
    /// </summary>
    public partial class ProfilesFiltersView
    {
        public ProfilesFiltersView()
        {
            InitializeComponent();
        }
        private void ProfileTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TryOpenPopUp();
        }

        private void TryOpenPopUp()
        {
            if (ProfileNamesItemsControl.Items.OfType<object>().Any())
            {
                MessageFilterPopup.IsOpen = true;
            }
        }

        private void ProfileTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TryClosePopUp();
        }

        private void TryClosePopUp()
        {
            if (!ProfileNamesItemsControl.Items.OfType<object>().Any())
            {
                MessageFilterPopup.IsOpen = false;
                return;
            }
            if (!MessageFilterPopup.IsFocused && !ProfileTextBox.IsFocused)
            {
                MessageFilterPopup.IsOpen = false;
            }
        }

        private void ProfileTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProfileTextBox.Text = "";
                TryOpenPopUp();
            }
            else if (e.Key == Key.Escape)
            {
                MessageFilterPopup.IsOpen = false;
            }
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TryClosePopUp();
        }
        
    }
}
