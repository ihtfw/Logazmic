#Logazmic
========

Minimalistic log viewer for Windows. Supports only log4j xml layout yet.

##Supports:
* Listnening on tcp port(4505)
* Opening *.log4j files 
* Drag-and-drop files

##Screenshots:
![Alt Logazmic screenshot 1](https://raw.githubusercontent.com/ihtfw/Logazmic/master/docs/screenshot1.png) ![Alt Logazmic screenshot 1](https://raw.githubusercontent.com/ihtfw/Logazmic/master/docs/screenshot2.png)

##Install link 

http://logazmic.azurewebsites.net/

##Setup for NLog (http://nlog-project.org/):
```csharp
var config = new LoggingConfiguration(); 

#region file
var ftXml = new FileTarget
                        {
                            FileName = XmlLogPath,
                            Layout = " ${log4jxmlevent}",
                            Encoding = Encoding.UTF8,
                            ArchiveEvery = FileArchivePeriod.Day,
                            ArchiveNumbering = ArchiveNumberingMode.Rolling
                        };

var asXml = new AsyncTargetWrapper(ftXml);
var ruleXml = new LoggingRule("*", LogLevel.Trace, asXml);
config.LoggingRules.Add(ruleXml);
#endregion

#region tcp
var tcpNetworkTarget = new NLogViewerTarget
                                   {
                                       Address = "tcp4://127.0.0.1:4505",
                                       Encoding = Encoding.UTF8,
                                       Name = "NLogViewer",
                                       IncludeNLogData = false
                                   };
var tcpNetworkRule = new LoggingRule("*", LogLevel.Trace, tcpNetworkTarget);
config.LoggingRules.Add(tcpNetworkRule);
#endregion

LogManager.Configuration = config;
```
