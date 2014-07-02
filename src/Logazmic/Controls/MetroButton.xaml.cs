namespace Logazmic.Controls
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MetroButton.xaml
    /// </summary>
    public partial class MetroButton
    {
        public static readonly DependencyProperty VisualProperty = VisualBrush.VisualProperty.AddOwner(typeof(MetroButton));

        public Visual Visual { get { return (Visual)GetValue(VisualProperty); } set { SetValue(VisualProperty, value); } }

        public MetroButton()
        {
            InitializeComponent();
        }
    }
}
