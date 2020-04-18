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

            Assert.AreEqual(LogLevel.Info, logEvent.LogLevel);
            Assert.AreEqual(text, logEvent.Message);
        }
        
        [Test]
        public void SplitToLogEventParseItems()
        {
            var sut = new FlatLogParser();

            var text = "123";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(1));
            Assert.AreEqual(new LogEventParseItem(0, text.Length), items.First());
        }

        [Test]
        public void SplitToLogEventParseItems_TwoItems()
        {
            var sut = new FlatLogParser();

            var text = "123\n34";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(2));
            Assert.AreEqual(new LogEventParseItem(0, 3), items[0]);
            Assert.AreEqual(new LogEventParseItem(4, 2), items[1]);
        }

        [Test]
        public void SplitToLogEventParseItems_ThreeItems()
        {
            var sut = new FlatLogParser();

            var text = "123\n34\nq";
            var items = sut.SplitToLogEventParseItems(text).ToList();

            Assert.That(items.Count, Is.EqualTo(3));
            Assert.AreEqual(new LogEventParseItem(0, 3), items[0]);
            Assert.AreEqual(new LogEventParseItem(4, 2), items[1]);
            Assert.AreEqual(new LogEventParseItem(7, 1), items[2]);
        }
    }
}
