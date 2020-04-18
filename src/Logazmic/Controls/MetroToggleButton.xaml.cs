namespace Logazmic.Controls
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MetroButton.xaml
    /// </summary>
    public partial class MetroToggleButton 
    {
        public static readonly DependencyProperty VisualProperty = VisualBrush.VisualProperty.AddOwner(typeof(MetroToggleButton));

        public Visual Visual { get => (Visual)GetValue(VisualProperty);
            set => SetValue(VisualProperty, value);
        }
        
        public MetroToggleButton()
        {
            InitializeComponent();
        }
    }
}
