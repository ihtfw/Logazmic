using System;
using Logazmic.Core.Filters;
using Logazmic.Core.Log;
using Logazmic.ViewModels;
using Logazmic.ViewModels.Events;
using Logazmic.ViewModels.Filters;
using Caliburn.Micro;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Logazmic.Tests.Core.Filters
{
    [TestFixture, SetCulture("ru-RU")]
    public class TimeFilterTests
    {
        private static readonly DateTime Start = new DateTime(2026, 1, 28, 8, 0, 20);

        [TestCase(null, null, -1, false)]
        [TestCase(0, null, -1, true)]
        [TestCase(0, null, 0, false)]
        [TestCase(null, 10, 11, true)]
        [TestCase(null, 10, 10, false)]
        [TestCase(0, 10, 5, false)]
        [TestCase(0, 10, 0, false)]
        [TestCase(0, 10, 10, false)]
        [TestCase(0, 10, -1, true)]
        [TestCase(0, 10, 11, true)]
        [TestCase(0, 0, 0, false)]
        public void FilterLogic_UsesInclusiveOptionalBounds(int? start, int? end, int offset, bool filtered)
        {
            var profile = new FiltersProfile
            {
                StartTime = start.HasValue ? Start.AddTicks(start.Value) : (DateTime?)null,
                EndTime = end.HasValue ? Start.AddTicks(end.Value) : (DateTime?)null
            };
            var logic = new FilterLogic(profile);
            logic.RebuildLeaves();

            Assert.That(logic.IsFiltered(new LogMessage
            {
                TimeStamp = Start.AddTicks(offset),
                LoggerName = string.Empty,
                Message = "Message",
                LogLevel = LogLevel.Info
            }), Is.EqualTo(filtered));
        }

        [Test]
        public void ViewModel_AppliesValidInputAndClearsBounds()
        {
            var profile = new FiltersProfile();
            var services = new LogPaneServices();
            var listener = new RefreshListener();
            services.EventAggregator.Subscribe(listener);
            var model = new TimeFilterViewModel(services, profile);

            model.StartTime = "28.01.2026 08:00:20";
            model.EndTime = "28.01.2026 08:00:21.1234567";

            Assert.That(profile.StartTime, Is.EqualTo(Start));
            Assert.That(profile.EndTime, Is.EqualTo(Start.AddSeconds(1).AddTicks(1234567)));
            Assert.That(model.Error, Is.Null);
            Assert.That(listener.Count, Is.EqualTo(2));

            model.StartTime = " ";
            model.EndTime = "";
            Assert.That(profile.StartTime, Is.Null);
            Assert.That(profile.EndTime, Is.Null);
            Assert.That(listener.Count, Is.EqualTo(4));
        }

        [Test]
        public void ViewModel_InvalidInputPreservesLastRangeUntilCorrected()
        {
            var profile = new FiltersProfile { StartTime = Start, EndTime = Start.AddHours(1) };
            var model = new TimeFilterViewModel(new LogPaneServices(), profile);

            model.StartTime = "31.02.2026 08:00:20";
            Assert.That(model.Error, Is.Not.Empty);
            Assert.That(profile.StartTime, Is.EqualTo(Start));

            model.StartTime = "28.01.2026 10:00:20";
            Assert.That(model.Error, Is.Not.Empty);
            Assert.That(profile.StartTime, Is.EqualTo(Start));

            model.EndTime = "28.01.2026 11:00:20";
            Assert.That(model.Error, Is.Null);
            Assert.That(profile.StartTime, Is.EqualTo(Start.AddHours(2)));
            Assert.That(profile.EndTime, Is.EqualTo(Start.AddHours(3)));
        }

        [Test]
        public void Profile_RoundTripsAndRefreshesEditor()
        {
            var saved = new FiltersProfile { StartTime = Start.AddTicks(1234567), EndTime = Start.AddDays(1) };
            var profile = new FiltersProfile();
            var model = new ProfileFiltersViewModel(profile, new LogPaneServices());
            model.TimeFilter.StartTime = "invalid";

            profile.Apply(JsonConvert.DeserializeObject<FiltersProfile>(JsonConvert.SerializeObject(saved)));
            model.UpdateFilters();

            Assert.That(profile.StartTime, Is.EqualTo(saved.StartTime));
            Assert.That(profile.EndTime, Is.EqualTo(saved.EndTime));
            Assert.That(model.TimeFilter.StartTime, Is.EqualTo("28.01.2026 08:00:20.1234567"));
            Assert.That(model.TimeFilter.EndTime, Is.EqualTo("29.01.2026 08:00:20"));
            Assert.That(model.TimeFilter.Error, Is.Null);

            profile.Apply(JsonConvert.DeserializeObject<FiltersProfile>("{}"));
            model.UpdateFilters();
            Assert.That(model.TimeFilter.StartTime, Is.Null);
            Assert.That(model.TimeFilter.EndTime, Is.Null);
        }

        [Test]
        public void FilterLogic_UsesStoredTimeAndCombinesOtherFilters()
        {
            var profile = new FiltersProfile { StartTime = Start, EndTime = Start };
            var logic = new FilterLogic(profile);
            logic.RebuildLeaves();
            var message = new LogMessage
            {
                TimeStamp = Start,
                LoggerName = string.Empty,
                Message = "Message",
                LogLevel = LogLevel.Info
            };

            Assert.That(logic.IsFiltered(message), Is.False);
            profile.FilterText = "different";
            Assert.That(logic.IsFiltered(message), Is.True);
            profile.FilterText = null;
            profile.MinLogLevel = LogLevel.Error;
            Assert.That(logic.IsFiltered(message), Is.True);
        }
        [Test]
        public void ViewModel_MasksCompactInputImmediately()
        {
            var profile = new FiltersProfile();
            var model = new TimeFilterViewModel(new LogPaneServices(), profile);
            model.StartTime = "28012026080000";
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:00"));
            Assert.That(profile.StartTime, Is.EqualTo(new DateTime(2026, 1, 28, 8, 0, 0)));
            Assert.That(model.Error, Is.Null);
        }

        [Test]
        public void AutomaticBounds_FollowBatchesWithoutRestrictingIncomingMessages()
        {
            var profile = new FiltersProfile();
            var model = new TimeFilterViewModel(new LogPaneServices(), profile);
            model.IncludeRange(Start, Start);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:20"));
            Assert.That(model.EndTime, Is.EqualTo(model.StartTime));

            model.IncludeRange(Start.AddSeconds(-1), Start.AddDays(1));
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:19"));
            Assert.That(model.EndTime, Is.EqualTo("29.01.2026 08:00:20"));
            Assert.That(profile.StartTime, Is.Null);
            Assert.That(profile.EndTime, Is.Null);

            model.StartTime = "28012026080000";
            model.IncludeRange(Start.AddDays(-1), Start.AddDays(2));
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:00"));
            Assert.That(model.EndTime, Is.EqualTo("30.01.2026 08:00:20"));
            Assert.That(profile.EndTime, Is.Null);

            model.EndTime = "";
            model.IncludeRange(Start, Start.AddDays(3));
            Assert.That(model.EndTime, Is.Empty);
            model.ClearAvailableRange();
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:00"));
            Assert.That(model.EndTime, Is.Empty);
        }

        [Test]
        public void AutomaticBounds_ResetOnClearAndRestoreWhenSelectingDefaultProfile()
        {
            var profile = new FiltersProfile();
            var model = new TimeFilterViewModel(new LogPaneServices(), profile);
            model.IncludeRange(Start, Start.AddDays(1));
            model.ClearAvailableRange();
            Assert.That(model.StartTime, Is.Null);
            Assert.That(model.EndTime, Is.Null);

            model.IncludeRange(Start, Start.AddDays(1));
            model.StartTime = "invalid";
            model.UpdateFilters();
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:20"));
            Assert.That(model.HasError, Is.False);
        }
        [TestCase(DateTimeKind.Unspecified)]
        [TestCase(DateTimeKind.Utc)]
        public void LocalInput_IsConvertedOnceToStoredBounds(DateTimeKind kind)
        {
            var zone = TimeZoneInfo.CreateCustomTimeZone("Test UTC+7", TimeSpan.FromHours(7), "Test UTC+7", "Test UTC+7");
            var raw = DateTime.SpecifyKind(Start, kind);
            var profile = new FiltersProfile();
            var model = new TimeFilterViewModel(new LogPaneServices(), profile, zone);
            model.SetUtcTime(false);
            model.IncludeRange(raw, raw.AddHours(1));
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 15:00:20"));
            Assert.That(model.EndTime, Is.EqualTo("28.01.2026 16:00:20"));

            model.StartTime = "28.01.2026 15:10:20";
            model.EndTime = "28.01.2026 15:20:20";
            Assert.That(profile.StartTime, Is.EqualTo(raw.AddMinutes(10)));
            Assert.That(profile.StartTime.Value.Kind, Is.EqualTo(kind));
            Assert.That(profile.EndTime, Is.EqualTo(raw.AddMinutes(20)));
            var logic = new FilterLogic(profile);
            logic.RebuildLeaves();
            var message = new LogMessage { TimeStamp = raw.AddMinutes(15), LoggerName = string.Empty, Message = "Message", LogLevel = LogLevel.Info };
            Assert.That(logic.IsFiltered(message), Is.False);
            message.TimeStamp = raw.AddMinutes(9);
            Assert.That(logic.IsFiltered(message), Is.True);

            model.SetUtcTime(true);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:10:20"));
            Assert.That(model.EndTime, Is.EqualTo("28.01.2026 08:20:20"));
            Assert.That(profile.StartTime, Is.EqualTo(raw.AddMinutes(10)));
            Assert.That(logic.IsFiltered(message), Is.True);
            model.SetUtcTime(false);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 15:10:20"));

            var saved = JsonConvert.DeserializeObject<FiltersProfile>(JsonConvert.SerializeObject(profile));
            profile.Apply(saved);
            model.UpdateFilters();
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 15:10:20"));
            Assert.That(profile.StartTime, Is.EqualTo(raw.AddMinutes(10)));

            model.StartTime = string.Empty;
            model.SetUtcTime(true);
            Assert.That(model.StartTime, Is.Null.Or.Empty);
            Assert.That(profile.StartTime, Is.Null);
        }

        [Test]
        public void LocalTimestamps_RemainInTheirOriginalRepresentation()
        {
            var zone = TimeZoneInfo.CreateCustomTimeZone("Test UTC+7", TimeSpan.FromHours(7), "Test UTC+7", "Test UTC+7");
            var raw = DateTime.SpecifyKind(Start, DateTimeKind.Local);
            var profile = new FiltersProfile();
            var model = new TimeFilterViewModel(new LogPaneServices(), profile, zone);
            model.IncludeRange(raw, raw.AddHours(1));
            model.SetUtcTime(false);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:20"));
            model.StartTime = "28.01.2026 08:10:20";
            Assert.That(profile.StartTime, Is.EqualTo(raw.AddMinutes(10)));
            Assert.That(profile.StartTime.Value.Kind, Is.EqualTo(DateTimeKind.Local));
            model.SetUtcTime(true);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:10:20"));
        }

        [Test]
        public void AutomaticRange_ChangesDisplayWithoutChangingStoredBounds()
        {
            var zone = TimeZoneInfo.CreateCustomTimeZone("Test UTC+7", TimeSpan.FromHours(7), "Test UTC+7", "Test UTC+7");
            var profile = new FiltersProfile();
            var model = new TimeFilterViewModel(new LogPaneServices(), profile, zone);
            model.IncludeRange(Start, Start.AddHours(1));
            model.SetUtcTime(false);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 15:00:20"));
            Assert.That(model.EndTime, Is.EqualTo("28.01.2026 16:00:20"));
            model.IncludeRange(Start, Start.AddHours(2));
            Assert.That(model.EndTime, Is.EqualTo("28.01.2026 17:00:20"));
            model.SetUtcTime(true);
            Assert.That(model.StartTime, Is.EqualTo("28.01.2026 08:00:20"));
            Assert.That(model.EndTime, Is.EqualTo("28.01.2026 10:00:20"));
            Assert.That(profile.StartTime, Is.Null);
            Assert.That(profile.EndTime, Is.Null);
        }
        private class RefreshListener : IHandle<RefreshEvent>
        {
            public int Count { get; private set; }

            public void Handle(RefreshEvent message)
            {
                Assert.That(message, Is.SameAs(RefreshEvent.Partial));
                Count++;
            }
        }
    }
}
