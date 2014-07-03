namespace Logazmic.Views
{
    using Caliburn.Micro;

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView
    {
        public SettingsView()
        {
            if (Execute.InDesignMode)
                IsOpen = true;
            InitializeComponent();
        }
    }
}
