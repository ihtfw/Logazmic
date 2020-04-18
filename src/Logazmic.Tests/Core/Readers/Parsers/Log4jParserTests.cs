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

            Assert.AreEqual("My.Super.App", logEvent.LoggerName);
            Assert.AreEqual(new []{"My", "Super", "App"}, logEvent.LoggerNames);
            Assert.AreEqual("App", logEvent.LastLoggerName);
            Assert.AreEqual(LogLevel.Info, logEvent.LogLevel);
            Assert.AreEqual("Hello world!", logEvent.Message);
            Assert.AreEqual(Log4JParser.ToDateTime(1574396643885), logEvent.TimeStamp);
            Assert.AreEqual("1", logEvent.ThreadName);
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
            Assert.AreEqual(new LogEventParseItem(0, text.Length), items.First());
        }

        [Test]
        public void SplitToLogEventParseItems_TwoItems()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var items = sut.SplitToLogEventParseItems(text + text).ToList();

            Assert.That(items.Count, Is.EqualTo(2));
            Assert.AreEqual(new LogEventParseItem(0, text.Length), items[0]);
            Assert.AreEqual(new LogEventParseItem(text.Length, text.Length), items[1]);
        }

        [Test]
        public void SplitToLogEventParseItems_WithTail()
        {
            var sut = new Log4JParser();

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var tail = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:pro";
            var items = sut.SplitToLogEventParseItems(text + tail).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.AreEqual(new LogEventParseItem(0, text.Length), items.First());
        }
        
        [Test]
        public void SplitToLogEventParseItems_TwoOpenings()
        {
            var sut = new Log4JParser();

            var opening = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1"">";
            var text = $@"{opening}{opening}<log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.AreEqual(new LogEventParseItem(opening.Length, text.Length - opening.Length), items.First());
        }

    }
}
