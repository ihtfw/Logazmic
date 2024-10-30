using Microsoft.Xaml.Behaviors;

namespace Logazmic
{
    using System;
    using System.Linq;
    using System.Windows.Input;

    internal static class ShortcutParser
    {
        public static bool CanParse(string triggerText)
        {
            return !string.IsNullOrWhiteSpace(triggerText) && triggerText.Contains("Shortcut");
        }

        public static TriggerBase CreateTrigger(string triggerText)
        {
            var triggerDetail = triggerText
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("Shortcut", string.Empty)
                .Trim();

            var modKeys = ModifierKeys.None;

            var allKeys = triggerDetail.Split('+');
            var key = (Key)Enum.Parse(typeof(Key), allKeys.Last());

            foreach (var modifierKey in allKeys.Take(allKeys.Count() - 1))
            {
                modKeys |= (ModifierKeys)Enum.Parse(typeof(ModifierKeys), modifierKey);
            }

            var keyBinding = new KeyBinding(new InputBindingTrigger(), key, modKeys);
            var trigger = new InputBindingTrigger { InputBinding = keyBinding };
            return trigger;
        }
    }
}