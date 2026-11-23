using Logazmic.Utils;

using NUnit.Framework;

using System;

namespace Logazmic.Tests.Core.Filters
{
    [TestFixture, SetCulture("ru-RU")]
    public class DateTimeEditingTests
    {

        [TestCase(0, 1, "29.01.2026 08:01:35.525088")]
        [TestCase(3, -1, "28.12.2025 08:01:35.525088")]
        [TestCase(6, 1, "28.01.2027 08:01:35.525088")]
        [TestCase(11, 1, "28.01.2026 09:01:35.525088")]
        [TestCase(14, -1, "28.01.2026 07:51:35.525088")]
        [TestCase(17, 1, "28.01.2026 08:01:45.525088")]
        [TestCase(20, 1, "28.01.2026 08:01:35.625088")]
        [TestCase(21, -1, "28.01.2026 08:01:35.515088")]
        [TestCase(22, 1, "28.01.2026 08:01:35.526088")]
        [TestCase(23, 1, "28.01.2026 08:01:35.525188")]
        [TestCase(23, -1, "28.01.2026 08:01:35.524988")]
        [TestCase(24, 1, "28.01.2026 08:01:35.525098")]
        [TestCase(25, 1, "28.01.2026 08:01:35.525089")]
        [TestCase(26, 1, "28.01.2026 08:01:35.525089")]
        [TestCase(15, -1, "28.01.2026 08:00:35.525088")]
        [TestCase(16, -1, "28.01.2026 08:00:35.525088")]
        [TestCase(18, 1, "28.01.2026 08:01:36.525088")]
        [TestCase(1, 1, "29.01.2026 08:01:35.525088")]
        [TestCase(4, -1, "28.12.2025 08:01:35.525088")]
        [TestCase(9, 1, "28.01.2027 08:01:35.525088")]
        [TestCase(12, 1, "28.01.2026 09:01:35.525088")]
        public void AdjustSection_ChangesSelectedUnit(int caret, int delta, string expected)
        {
            Assert.That(DateTimeInput.TryAdjustSection("28.01.2026 08:01:35.525088", caret, delta, out var result), Is.True);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("31.01.2026 23:59:59", 3, 1, "28.02.2026 23:59:59")]
        [TestCase("28.02.2024 23:59:59", 18, 1, "29.02.2024 00:00:00")]
        [TestCase("29.02.2024 08:00:00", 6, 1, "28.02.2025 08:00:00")]
        public void AdjustSection_HandlesCalendarBoundaries(string input, int caret, int delta, string expected)
        {
            Assert.That(DateTimeInput.TryAdjustSection(input, caret, delta, out var result), Is.True);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("01.01.0001 00:00:00", -1)]
        [TestCase("31.12.9999 23:59:59", 1)]
        [TestCase("28.01", 1)]
        public void AdjustSection_RejectsIncompleteDatesAndOverflow(string input, int delta)
        {
            Assert.That(DateTimeInput.TryAdjustSection(input, 0, delta, out var result), Is.False);
            Assert.That(result, Is.EqualTo(input));
        }

        [Test, SetCulture("en-US")]
        public void AdjustSection_UsesRegionalSectionOrder()
        {
            Assert.That(DateTimeInput.TryAdjustSection("01/28/2026 08:01:35", 0, 1, out var result), Is.True);
            Assert.That(result, Is.EqualTo("02/28/2026 08:01:35"));
        }

        [TestCase("2", "2")]
        [TestCase("280", "28.0")]
        [TestCase("28012026", "28.01.2026")]
        [TestCase("2801202608", "28.01.2026 08")]
        [TestCase("28012026080000", "28.01.2026 08:00:00")]
        [TestCase("28.01.2026 08:00:00.", "28.01.2026 08:00:00.")]
        [TestCase("28012026080000123", "28.01.2026 08:00:00.123")]
        [TestCase("invalid", "invalid")]
        [SetCulture("ru-RU")]
        public void Mask_FormatsPartialInput(string input, string expected)
        {
            Assert.That(DateTimeInput.ApplyMask(input), Is.EqualTo(expected));
        }

        [Test, SetCulture("en-US")]
        public void Mask_UsesOperatingSystemDateOrderAndSeparator()
        {
            var text = DateTimeInput.ApplyMask("01282026080000");
            Assert.That(text, Is.EqualTo("01/28/2026 08:00:00"));
            Assert.That(DateTimeInput.TryParse(text, out var value), Is.True);
            Assert.That(value, Is.EqualTo(new DateTime(2026, 1, 28, 8, 0, 0)));
        }
    }
}
