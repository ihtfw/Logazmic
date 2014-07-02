namespace Logazmic.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    using Logazmic.Core.Log;

    internal class LogLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var logLevel = (LogLevel)value;
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return Brushes.DarkGray;
                    case LogLevel.Info:
                        return Brushes.Green;
                    case LogLevel.Warn:
                        return Brushes.Orange;
                    case LogLevel.Error:
                        return Brushes.Red;
                    case LogLevel.Fatal:
                        return Brushes.Purple;
                    default:
                        return Brushes.Black;
                }
            }
            catch(Exception)
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}