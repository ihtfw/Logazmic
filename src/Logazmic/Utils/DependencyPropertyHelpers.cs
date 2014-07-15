using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logazmic.Utils
{
    using System.Windows;
    using System.Windows.Media;

    public static class DependencyPropertyHelpers
    {
        public static T FindParent<T>(this DependencyObject start) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(start);
            if (parentObject == null)
            {
                return null;
            }

            var parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
    }
}
