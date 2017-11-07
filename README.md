Logazmic
========
<img src="https://raw.githubusercontent.com/ihtfw/Logazmic/master/docs/appbar.flag.bear.png" alt="Icon" width="64px" height="64px" />
<img src="https://ci.appveyor.com/api/projects/status/ns7gdofc8w4so32m?svg=true" width="300">


Minimalistic log viewer for nlog. Supports only log4j xml layout yet. Core is based on [Log2console](https://log2console.codeplex.com/). UI is rewritten in WPF with usage of [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)

## Supports:
* Listening on tcp/udp
* Opening *.log4j files 
* Drag-and-drop files

## Download 
https://github.com/ihtfw/Logazmic/releases/latest/

## Screenshots:

![Alt Logazmic screenshot 1](https://raw.githubusercontent.com/ihtfw/Logazmic/master/docs/screenshot1.png) ![Alt Logazmic screenshot 1](https://raw.githubusercontent.com/ihtfw/Logazmic/master/docs/screenshot2.png)



## Setup 
### NLog (http://nlog-project.org/):
### Xml configuration
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <targets>
        <target name="logfile"  layout="${log4jxmlevent}"  xsi:type="File" fileName="file.txt" />
    </targets>

    <rules>
        <logger name="*" minlevel="Info" writeTo="logfile" />
    </rules>
</nlog>
```
### Code configuration
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
<!---
### Log4net
```xml
<appender name=\"FileAppender\" type=\"log4net.Appender.FileAppender\">
    <file value=\"log-file.txt\" />
    <appendToFile value=\"true\" />
    <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />
    <layout type=\"log4net.Layout.XmlLayoutSchemaLog4j\" />
</appender>
```
-->

### Logazmic.Integration
<img src="https://raw.githubusercontent.com/ihtfw/Logazmic/master/docs/appbar.flag.bear.png" alt="Icon" width="64px" height="64px" />
<img src="https://ci.appveyor.com/api/projects/status/n45ye34e920c7c5k?svg=true" width="300">

Install nuget package <a href="https://www.nuget.org/packages/Logazmic.Integration/">Logazmic.Integration</a>. In your code to install, run and open log file
```csharp
 var runner = new Logazmic.Integration.Runner();
 runner.Run(pathToLogFile);
```
