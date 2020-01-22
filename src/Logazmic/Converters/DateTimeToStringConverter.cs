using System;
using System.Globalization;
using System.Windows.Data;

namespace Logazmic.Converters
{
    public interface IDateTimeToStringConverterOptions
    {
        bool UtcTime { get; }

        bool Use24HourFormat { get; }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public IDateTimeToStringConverterOptions Options { get; set; }

        public string Format(DateTime dateTime)
        {
            if (Options.Use24HourFormat)
            {
                return dateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            
            return dateTime.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                if (Options.UtcTime)
                {
                    return Format(dateTime);
                }

                return Format(dateTime.ToLocalTime());
            }

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
