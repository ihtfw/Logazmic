using System.Windows.Controls;

namespace Logazmic.Views
{
    using Caliburn.Micro;

    /// <summary>
    /// Interaction logic for LogPaneView.xaml
    /// </summary>
    public partial class LogPaneView
    {
        private ScrollViewer scrollViewer;

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
    }
}
