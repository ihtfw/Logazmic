using System.Collections.Generic;
using System.IO;
using Logazmic.Core.Log;

namespace Logazmic.Core.Readers
{
    public interface ILogStreamReader
    {
        string DefaultLogger { get; set; }
        int BufferSize { get; set; }

        IEnumerable<LogMessage> NextLogEvents(Stream stream, out int bytesRead);
    }
}