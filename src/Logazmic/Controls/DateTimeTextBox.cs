using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Logazmic.Utils;

namespace Logazmic.Controls
{
    public class DateTimeTextBox : TextBox
    {
        private bool _applyingMask;

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(DateTime?), typeof(DateTimeTextBox));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(DateTime?), typeof(DateTimeTextBox));

        public DateTime? Minimum
        {
            get => (DateTime?)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public DateTime? Maximum
        {
            get => (DateTime?)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && e.Text.All(char.IsDigit)
                && SelectionLength != Text.Length)
            {
                var text = Text;
                var position = SelectionStart;
                foreach (var digit in e.Text)
                {
                    while (position < text.Length && !char.IsDigit(text[position]))
                        position++;

                    text = position < text.Length
                        ? text.Remove(position, 1).Insert(position, digit.ToString())
                        : text + digit;
                    position++;
                }

                var digitsBeforeCaret = text.Take(position).Count(char.IsDigit);
                SetCurrentValue(TextProperty, text);
                position = 0;
                while (position < Text.Length && digitsBeforeCaret > 0)
                {
                    if (char.IsDigit(Text[position++]))
                        digitsBeforeCaret--;
                }
                Select(position, 0);
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.Up || e.Key == Key.Down) && Keyboard.Modifiers == ModifierKeys.None)
            {
                var caret = SelectionStart;
                var selectionLength = SelectionLength;
                if (DateTimeInput.TryAdjustSection(Text, caret, e.Key == Key.Up ? 1 : -1, out var adjusted)
                    && DateTimeInput.TryParse(adjusted, out var value)
                    && (!Minimum.HasValue || value >= Minimum)
                    && (!Maximum.HasValue || value <= Maximum))
                {
                    SetCurrentValue(TextProperty, adjusted);
                    Select(Math.Min(caret, Text.Length), Math.Min(selectionLength, Text.Length - Math.Min(caret, Text.Length)));
                }
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            GetBindingExpression(TextProperty)?.UpdateSource();
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (_applyingMask)
                return;

            var formatted = DateTimeInput.ApplyMask(Text);
            if (formatted != Text)
            {
                var digitsBeforeCaret = Text.Take(CaretIndex).Count(char.IsDigit);
                _applyingMask = true;
                try
                {
                    SetCurrentValue(TextProperty, formatted);
                    var caret = 0;
                    while (caret < formatted.Length && digitsBeforeCaret > 0)
                    {
                        if (char.IsDigit(formatted[caret++]))
                            digitsBeforeCaret--;
                    }
                    CaretIndex = caret;
                }
                finally
                {
                    _applyingMask = false;
                }
            }
            base.OnTextChanged(e);
        }
    }
}
