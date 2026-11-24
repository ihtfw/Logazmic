using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Logazmic.Utils
{
    public static class DateTimeInput
    {
        public static string Format => Regex.Replace(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
            "d+|M+|y+", match => match.Value[0] == 'y' ? "yyyy" : new string(match.Value[0], 2)) + " HH:mm:ss";

        public static string FormatValue(DateTime? value)
        {
            return value?.ToString(Format + ".FFFFFFF", CultureInfo.CurrentCulture);
        }

        public static string ApplyMask(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var mask = Regex.Replace(Format, "d+|M+|y+|H+|m+|s+", match => new string('0', match.Length))
                .Replace("/", CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator)
                .Replace(":", CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator)
                .Replace("'", string.Empty);
            if (text.Any(c => !char.IsDigit(c) && !mask.Contains(c) && c != '.'))
                return text;

            var digits = new string(text.Where(char.IsDigit).ToArray());
            if (digits.Length > 21)
                return text;

            var result = new StringBuilder();
            var index = 0;
            foreach (var character in mask)
            {
                if (index == digits.Length)
                    break;
                result.Append(character == '0' ? digits[index++] : character);
            }
            if (index < digits.Length)
                result.Append('.').Append(digits.Substring(index));
            else if (digits.Length == 14 && text.EndsWith("."))
                result.Append('.');
            return result.ToString();
        }

        public static bool TryParse(string text, out DateTime? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(text))
                return true;

            if (!DateTime.TryParseExact(text, Format + ".FFFFFFF", CultureInfo.CurrentCulture,
                DateTimeStyles.None, out var parsed))
                return false;

            value = parsed;
            return true;
        }

        public static bool TryAdjustSection(string text, int caret, int delta, out string adjusted)
        {
            adjusted = text;
            if (!TryParse(text, out var parsed) || !parsed.HasValue)
                return false;

            var sections = Regex.Matches(text, @"\d+").Cast<Match>().ToArray();
            var section = Array.FindLastIndex(sections, match => match.Index <= caret);
            if (section < 0)
                section = 0;
            var formats = Regex.Matches(Format, "d+|M+|y+|H+|m+|s+").Cast<Match>().ToArray();
            // The digit to the right of the caret determines the step. At a separator
            // or at the end of the text, use the preceding section's last digit.
            var digit = Math.Max(0, Math.Min(caret - sections[section].Index, sections[section].Length - 1));
            var step = delta * (int)Math.Pow(10, sections[section].Length - digit - 1);
            var value = parsed.Value;
            try
            {
                if (section >= formats.Length)
                {
                    value = value.AddTicks(delta * (long)Math.Pow(10, 6 - digit));
                }
                else
                {
                    switch (formats[section].Value[0])
                    {
                        case 'd': value = value.AddDays(delta); break;
                        case 'M': value = value.AddMonths(delta); break;
                        case 'y': value = value.AddYears(delta); break;
                        case 'H': value = value.AddHours(delta); break;
                        case 'm': value = value.AddMinutes(step); break;
                        case 's': value = value.AddSeconds(step); break;
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            // Keep fractional precision so repeated arrow presses retain the same step.
            adjusted = value.ToString(Format + (sections.Length > formats.Length
                ? "." + new string('f', sections.Last().Length) : string.Empty), CultureInfo.CurrentCulture);
            return true;
        }
    }
}
