namespace Logazmic.Core.Log
{
    using System;

    [Serializable]
    public enum LogLevel
    {
        None = -1,
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}