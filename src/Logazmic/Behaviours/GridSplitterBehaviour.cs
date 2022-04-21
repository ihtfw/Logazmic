namespace Logazmic.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using Settings;
    using Utils;

    public class DtoGridLength
    {
        public DtoGridLength()
        {
        }

        private DtoGridLength(GridLength val)
        {
            Value = val.Value;
            GridUnitType = val.GridUnitType;
        }

        public double Value { get; set; }

        public GridUnitType GridUnitType { get; set; }

        public static implicit operator DtoGridLength(GridLength val)
        {
            return new DtoGridLength(val);
        }

        public static implicit operator GridLength(DtoGridLength val)
        {
            return new GridLength(val.Value, val.GridUnitType);
        }
    }

    public class GridSplitterData
    {
        private List<DtoGridLength> _rows;

        private List<DtoGridLength> _cols;

        public List<DtoGridLength> Rows { get => _rows ?? (_rows = new List<DtoGridLength>());
            set => _rows = value;
        }

        public List<DtoGridLength> Cols { get => _cols ?? (_cols = new List<DtoGridLength>());
            set => _cols = value;
        }

        public void Update(GridSplitter gs, Grid grid)
        {
            Rows.Clear();
            Cols.Clear();
            Rows.AddRange(grid.RowDefinitions.Select(d => (DtoGridLength)d.Height));
            Cols.AddRange(grid.ColumnDefinitions.Select(d => (DtoGridLength)d.Width));
        }

        public bool Restore(GridSplitter gs, Grid grid)
        {
            var any = false;
            if (Rows.Count == grid.RowDefinitions.Count)
            {
                for (int i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    var gridRow = grid.RowDefinitions[i];
                    gridRow.Height = Rows[i];
                }
                any = true;
            }
            if (Cols.Count == grid.ColumnDefinitions.Count)
            {
                for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                {
                    var gridCol = grid.ColumnDefinitions[i];
                    gridCol.Width = Cols[i];
                }
                any = true;
            }
            return any;
        }
    }

    public class GridSplitterSizes : JsonSettingsBase
    {
        #region Singleton

        private static readonly Lazy<GridSplitterSizes> LazyInstance = new Lazy<GridSplitterSizes>(() => Load<GridSplitterSizes>(SettingsFilePath));

        public static GridSplitterSizes Instance => LazyInstance.Value;

        #endregion

        private static readonly string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Logazmic\ui_settings.json");

        private Dictionary<string, GridSplitterData> _gridSplitters;
        
        public Dictionary<string, GridSplitterData> GridSplitters { get => _gridSplitters ?? (_gridSplitters = new Dictionary<string, GridSplitterData>());
            set => _gridSplitters = value;
        }

        public override void Save()
        {
            Save(SettingsFilePath);
        }
    }

    public static class GridSplitterBehaviour
    {
        /// <summary>
        /// Name must be unique. Best choice is use full name of view with namespace
        /// </summary>
        public static readonly DependencyProperty SaveSizeWithNameProperty = DependencyProperty.RegisterAttached(
            "SaveSizeWithName", typeof(string), typeof(GridSplitterBehaviour), new PropertyMetadata(default(string), SaveSizeWithNameChangedCallback));

        private static void SaveSizeWithNameChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var gs = (GridSplitter)obj;
            var grid = gs.FindParent<Grid>();
            if (grid == null)
            {
                throw new Exception("Can't find grid!");
            }

            GridSplitterData values;
            var sizes = GridSplitterSizes.Instance.GridSplitters;
            var name = args.NewValue.ToString();
            if (sizes.ContainsKey(name))
            {
                values = sizes[name];
            }
            else
            {
                values = new GridSplitterData();
                sizes.Add(name, values);
            }

            gs.DragCompleted += (sender, eventArgs) =>
            {
                try
                {
                    values.Update(gs, grid);
                }
                catch
                {
                    //ignore
                }
            };

            gs.Loaded += (sender, eventArgs) =>
            {
                try
                {
                    values.Restore(gs, grid);
                }
                catch
                {
                    //ignore
                }
            };
        }

        public static void SetSaveSizeWithName(DependencyObject element, string value)
        {
            element.SetValue(SaveSizeWithNameProperty, value);
        }

        public static string GetSaveSizeWithName(DependencyObject element)
        {
            return (string)element.GetValue(SaveSizeWithNameProperty);
        }
    }
}
