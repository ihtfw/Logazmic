namespace Logazmic
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public static readonly DependencyProperty InputBindingProperty =
            DependencyProperty.Register("InputBinding", typeof(InputBinding)
                , typeof(InputBindingTrigger)
                , new UIPropertyMetadata(null));

        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            // action is anyway blocked by Caliburn at the invoke level
            return true;
        }

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }

        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                if (AssociatedObject.Focusable)
                {
                    AssociatedObject.InputBindings.Add(InputBinding);
                }
                else
                {
                    FrameworkElement focusable = null;
                    AssociatedObject.Loaded += delegate
                    {
                        focusable = GetFocusable(AssociatedObject);
                        if (!focusable.InputBindings.Contains(InputBinding))
                        {
                            focusable.InputBindings.Add(InputBinding);
                        }
                    };
                    AssociatedObject.Unloaded += delegate
                    {
                        focusable.InputBindings.Remove(InputBinding);
                    };
                }
            }
            base.OnAttached();

        }

        private Window GetWindow(FrameworkElement frameworkElement)
        {
            if (frameworkElement is Window)
                return frameworkElement as Window;

            var parent = frameworkElement.Parent as FrameworkElement;
            Debug.Assert(parent != null);

            return GetWindow(parent);
        }

        private FrameworkElement GetFocusable(FrameworkElement frameworkElement)
        {
            if (frameworkElement.Focusable)
                return frameworkElement;

            var parent = frameworkElement.Parent as FrameworkElement;
            Debug.Assert(parent != null);

            return GetFocusable(parent);
        }
    }
}