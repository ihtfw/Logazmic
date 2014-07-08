using System.Windows;
using System.Windows.Media;

namespace Logazmic.Controls
{


    /// <summary>
    /// Interaction logic for CircleMenuButton.xaml
    /// </summary>
    public partial class CircleMenuButton
    {
        public static readonly DependencyProperty VisualProperty = DependencyProperty.Register(
            "Visual", typeof(Visual), typeof(CircleMenuButton), new PropertyMetadata(default(Visual)));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(CircleMenuButton), new PropertyMetadata(default(bool)));

        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }

        public Visual Visual { get { return (Visual)GetValue(VisualProperty); } set { SetValue(VisualProperty, value); } }

        public CircleMenuButton()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MouseRightButtonUp += (sender, args) => args.Handled = true;
        }
    }
}
