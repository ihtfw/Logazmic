using System;
using System.Linq;
using Logazmic.Core.Log;
using Logazmic.Core.Readers.Parsers;
using NUnit.Framework;

namespace Logazmic.Tests.Core.Readers.Parsers
{
    [TestFixture]
    public class Log4JParserTests
    {
        [Test]
        public void ParseLogEvent_Success_Test()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var logEvent = sut.ParseLogEvent(text);

            Assert.That(logEvent.LoggerName, Is.EqualTo("My.Super.App"));
            Assert.That(logEvent.LoggerNames, Is.EqualTo(new[] { "My", "Super", "App" }));
            Assert.That(logEvent.LastLoggerName, Is.EqualTo("App"));
            Assert.That(logEvent.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(logEvent.Message, Is.EqualTo("Hello world!"));
            Assert.That(logEvent.TimeStamp, Is.EqualTo(Log4JParser.ToDateTime(1574396643885)));
            Assert.That(logEvent.ThreadName, Is.EqualTo("1"));
        }

        [Test]
        public void ParseLogEvent_Fail_Test()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:even logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";

            Assert.Throws<Exception>(() =>
            {
                sut.ParseLogEvent(text);
            });
        }

        [Test]
        public void SplitToLogEventParseItems()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items.First(), Is.EqualTo(new LogEventParseItem(0, text.Length)));
        }

        [Test]
        public void SplitToLogEventParseItems_TwoItems()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var items = sut.SplitToLogEventParseItems(text + text).ToList();

            Assert.That(items.Count, Is.EqualTo(2));
            Assert.That(items[0], Is.EqualTo(new LogEventParseItem(0, text.Length)));
            Assert.That(items[1], Is.EqualTo(new LogEventParseItem(text.Length, text.Length)));
        }

        [Test]
        public void SplitToLogEventParseItems_WithTail()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var tail = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:pro";
            var items = sut.SplitToLogEventParseItems(text + tail).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items.First(), Is.EqualTo(new LogEventParseItem(0, text.Length)));
        }
        
        [Test]
        public void SplitToLogEventParseItems_TwoOpenings()
        {
            var sut = new Log4JParser();

            var opening = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1"">";
            var text = $@"{opening}{opening}<log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items.First(), Is.EqualTo(new LogEventParseItem(opening.Length, text.Length - opening.Length)));
        }

    }
}
