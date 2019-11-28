using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logazmic.Core.Readers;
using Logazmic.Core.Readers.Parsers;
using NUnit.Framework;

namespace Logazmic.Tests.Core.Readers
{
    public static class Utils
    {
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    [TestFixture]
    public class LogStreamReaderTests
    {
        [Test]
        public void NextLogEvents_Empty()
        {
            var sut = new LogStreamReader(new Log4jParser());

            using (var ms = new MemoryStream())
            {
                var logMessages = sut.NextLogEvents(ms).ToList();
                Assert.IsEmpty(logMessages);
            }
        }

        [Test]
        public void NextLogEvents_SingleEvent()
        {
            var sut = new LogStreamReader(new Log4jParser());

            var text = @"<log4j:event logger=""My.Super.App"" level=""INFO"" timestamp=""1574396643885"" thread=""1""><log4j:message>Hello world!</log4j:message><log4j:properties><log4j:data name=""log4japp"" value=""My.Super.APp.exe(7944)"" /><log4j:data name=""log4jmachinename"" value=""DESKTOP-E10B4T4"" /></log4j:properties></log4j:event>";
            using (var ms = Utils.GenerateStreamFromString(text))
            {
                var logMessages = sut.NextLogEvents(ms).ToList();
                Assert.That(logMessages.Count, Is.EqualTo(1));
            }
        }
    }
}
