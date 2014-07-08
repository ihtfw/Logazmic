using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logazmic.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public static class DataGridBehavior
    {
        public static readonly DependencyProperty AutoscrollProperty = DependencyProperty.RegisterAttached(
            "Autoscroll", typeof(bool), typeof(DataGridBehavior), new PropertyMetadata(default(bool),AutoscrollChangedCallback));

        private static void AutoscrollChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid == null)
            {
                throw new InvalidOperationException("Dependancy object is not DataGrid");
            }
            if ((bool)args.NewValue)
            {
                dataGrid.InitializingNewItem += ScrollToEnd;
                ScrollToEnd(dataGrid);
            }
            else
            {
                dataGrid.InitializingNewItem -= ScrollToEnd;
            }
        }

        private static void ScrollToEnd(object sender, InitializingNewItemEventArgs initializingNewItemEventArgs)
        {
            var datagrid = (DataGrid)sender;
            ScrollToEnd(datagrid);
        }

        private static void ScrollToEnd(DataGrid datagrid)
        {
            datagrid.ScrollIntoView(datagrid.Items[datagrid.Items.Count - 1]);
        }

        public static void SetAutoscroll(DependencyObject element, bool value)
        {
            element.SetValue(AutoscrollProperty, value);
        }

        public static bool GetAutoscroll(DependencyObject element)
        {
            return (bool)element.GetValue(AutoscrollProperty);
        }
    }
}
