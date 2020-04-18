using System;
using System.Text;
using System.Xml;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers.Parsers
{
    public class Log4JParser : AXmlLogParser
    {
        const string Log4JNamespace = "http://jakarta.apache.org/log4j/";
        const string NlogNamespace = "http://nlog-project.org";

        static readonly DateTime S1970 = new DateTime(1970, 1, 1);
        
        readonly XmlParserContext _xmlContext = CreateContext();

        public Log4JParser() : base("log4j:event")
        {
        }

        static XmlParserContext CreateContext()
        {
            var nameTable = new NameTable();
            var xmlNamespaceManager = new XmlNamespaceManager(nameTable);
            xmlNamespaceManager.AddNamespace("log4j", Log4JNamespace);
            xmlNamespaceManager.AddNamespace("nlog", NlogNamespace);
            return new XmlParserContext(nameTable, xmlNamespaceManager, "elem", XmlSpace.None, Encoding.UTF8);
        }
        

        /// <summary>
        /// Here we expect the log event to use the log4j schema.
        /// Sample:
        ///     <log4j:event logger="Statyk7.Another.Name.DummyManager" timestamp="1184286222308" level="ERROR" thread="1">
        ///         <log4j:message>This is an Message</log4j:message>
        ///         <log4j:properties>
        ///             <log4j:data name="log4jmachinename" value="remserver" />
        ///             <log4j:data name="log4net:HostName" value="remserver" />
        ///             <log4j:data name="log4net:UserName" value="REMSERVER\Statyk7" />
        ///             <log4j:data name="log4japp" value="Test.exe" />
        ///         </log4j:properties>
        ///     </log4j:event>
        /// </summary>
        /// 
        /// Implementation inspired from: http://geekswithblogs.net/kobush/archive/2006/04/20/75717.aspx
        /// 
        protected override LogMessage ParseLogEvent(XmlReader reader)
        {
            var moveResult = reader.MoveToContent();

            if ((moveResult != XmlNodeType.Element) || (reader.Name != "log4j:event"))
                throw new Exception("The Log Event is not a valid log4j Xml block.");
            
            // we are on event element - extract known attributes
            var logMsg = new LogMessage
            {
                LoggerName = reader.GetAttribute("logger"),
                LogLevel = (LogLevel) Enum.Parse(typeof(LogLevel), reader.GetAttribute("level") ?? nameof(LogLevel.Trace), true),
                ThreadName = reader.GetAttribute("thread")
            };
            
            if (long.TryParse(reader.GetAttribute("timestamp"), out var timeStamp))
                logMsg.TimeStamp = ToDateTime(timeStamp);
            
            int eventDepth = reader.Depth;
            reader.Read();
            while (reader.Depth > eventDepth)
            {
                if (reader.MoveToContent() == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "log4j:message":
                            logMsg.Message = reader.ReadString();
                            break;

                        case "log4j:throwable":
                            logMsg.Message += Environment.NewLine + reader.ReadString();
                            break;

                        case "log4j:locationInfo":
                            logMsg.CallSiteClass = reader.GetAttribute("class");
                            logMsg.CallSiteMethod = reader.GetAttribute("method");
                            logMsg.SourceFileName = reader.GetAttribute("file");
                            if (uint.TryParse(reader.GetAttribute("line"), out var sourceFileLine))
                                logMsg.SourceFileLineNr = sourceFileLine;
                            break;

                        case "nlog:eventSequenceNumber":
                            if (ulong.TryParse(reader.ReadString(), out var sequenceNumber))
                                logMsg.SequenceNr = sequenceNumber;
                            break;

                        case "nlog:locationInfo":
                            break;

                        case "log4j:properties":
                            reader.Read();
                            while (reader.MoveToContent() == XmlNodeType.Element
                                   && reader.Name == "log4j:data")
                            {
                                string name = reader.GetAttribute("name");
                                string value = reader.GetAttribute("value");
                                if (name != null)
                                {
                                    if (name.ToLower().Equals("exceptions"))
                                    {
                                        logMsg.ExceptionString = value;
                                    }
                                    else
                                    {
                                        logMsg.Properties[name] = value;
                                    }
                                }

                                reader.Read();
                            }

                            break;
                    }
                }
                reader.Read();
            }

            return logMsg;
        }

        protected override XmlParserContext GetXmlParserContext()
        {
            return _xmlContext;
        }

        public static DateTime ToDateTime(long timeStamp)
        {
            return S1970.AddMilliseconds(timeStamp);
        }
    }
}