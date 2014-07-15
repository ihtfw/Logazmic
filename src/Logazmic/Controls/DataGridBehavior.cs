namespace Logazmic.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public static class DataGridBehavior
    {
        class DataGridInfo
        {
            public DataGridInfo(NotifyCollectionChangedEventHandler handler, DispatcherTimer timer)
            {
                Handler = handler;
                Timer = timer;
            }

            public NotifyCollectionChangedEventHandler Handler { get; set; }
            public DispatcherTimer Timer { get; set; }
        }
        public static readonly DependencyProperty AutoscrollProperty = DependencyProperty.RegisterAttached(
            "Autoscroll", typeof(bool), typeof(DataGridBehavior), new PropertyMetadata(default(bool), AutoscrollChangedCallback));

        private static readonly Dictionary<DataGrid, DataGridInfo> infoDict = new Dictionary<DataGrid, DataGridInfo>();

        private static void AutoscrollChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid == null)
            {
                throw new InvalidOperationException("Dependency object is not DataGrid.");
            }

            if ((bool)args.NewValue)
            {
                Subscribe(dataGrid);
                dataGrid.Unloaded += DataGridOnUnloaded;
                dataGrid.Loaded += DataGridOnLoaded;
            }
            else
            {   
                Unsubscribe(dataGrid);
            dataGrid.Unloaded -= DataGridOnUnloaded;
            dataGrid.Loaded -= DataGridOnLoaded;
            }
        }

        private static void Subscribe(DataGrid dataGrid)
        {
            if (infoDict.ContainsKey(dataGrid))
                return;

            var timer = new DispatcherTimer(DispatcherPriority.Background);
            var handler = new NotifyCollectionChangedEventHandler((sender, eventArgs) => timer.Start());

            infoDict.Add(dataGrid, new DataGridInfo(handler, timer));

            timer.Tick += (sender, args) => ScrollToEnd(dataGrid);
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged += handler;
            
            timer.Start();
        }

        private static void Unsubscribe(DataGrid dataGrid)
        {
            DataGridInfo info;
            if (infoDict.TryGetValue(dataGrid, out info))
            {
                ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged -= info.Handler;
                infoDict.Remove(dataGrid);

                info.Timer.Stop();
            }
        }

        private static void DataGridOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            if (GetAutoscroll(dataGrid))
            {
                Subscribe(dataGrid);
            }
        }

        private static void DataGridOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            if (GetAutoscroll(dataGrid))
            {   
                Unsubscribe(dataGrid);
            }
        }

        private static void ScrollToEnd(DataGrid datagrid)
        {
            infoDict[datagrid].Timer.Stop();

            if (datagrid.Items.Count == 0)
            {
                return;
            }
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
