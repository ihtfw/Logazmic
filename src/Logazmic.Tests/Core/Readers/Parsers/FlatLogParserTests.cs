using System.Linq;
using Logazmic.Core.Log;
using Logazmic.Core.Readers.Parsers;
using NUnit.Framework;

namespace Logazmic.Tests.Core.Readers.Parsers
{
    [TestFixture]
    public class FlatLogParserTests
    {
        [Test]
        public void ParseLogEvent_Success_Test()
        {
            var sut = new FlatLogParser();

            var text = @"Hello";
            var logEvent = sut.ParseLogEvent(text);

            Assert.That(logEvent.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(logEvent.Message, Is.EqualTo(text));
        }
        
        [Test]
        public void SplitToLogEventParseItems()
        {
            var sut = new FlatLogParser();

            var text = "123";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items.First(), Is.EqualTo(new LogEventParseItem(0, text.Length)));
        }

        [Test]
        public void SplitToLogEventParseItems_TwoItems()
        {
            var sut = new FlatLogParser();

            var text = "123\n34";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(2));
            Assert.That(items[0], Is.EqualTo(new LogEventParseItem(0, 3)));
            Assert.That(items[1], Is.EqualTo(new LogEventParseItem(4, 2)));
        }

        [Test]
        public void SplitToLogEventParseItems_ThreeItems()
        {
            var sut = new FlatLogParser();

            var text = "123\n34\nq";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(3));
            Assert.That(items[0], Is.EqualTo(new LogEventParseItem(0, 3)));
            Assert.That(items[1], Is.EqualTo(new LogEventParseItem(4, 2)));
            Assert.That(items[2], Is.EqualTo(new LogEventParseItem(7, 1)));
        }
    }
}
