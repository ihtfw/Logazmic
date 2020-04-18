using System.Windows;
using System.Windows.Media;

namespace Logazmic.Controls
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Markup;

    /// <summary>
    /// Interaction logic for CircleMenuButton.xaml
    /// </summary>
    [ContentProperty("Items")]
    [TemplatePart(Name = "PART_ToggleButton", Type = typeof(ContextMenu))]
    public partial class CircleMenuButton
    {
        public static readonly DependencyProperty VisualProperty = DependencyProperty.Register(
            "Visual", typeof(Visual), typeof(CircleMenuButton), new PropertyMetadata(default(Visual)));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(CircleMenuButton), new PropertyMetadata(default(bool)));

        private ToggleButton _toggleButton;

        public bool IsExpanded { get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public Visual Visual { get => (Visual)GetValue(VisualProperty);
            set => SetValue(VisualProperty, value);
        }

        public CircleMenuButton()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public new ItemCollection Items => _toggleButton.ContextMenu?.Items;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _toggleButton = (ToggleButton)GetTemplateChild("PART_ToggleButton");
            MouseRightButtonUp += (sender, args) => args.Handled = true;
        }
    }
}
