using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Logazmic.Core.Log;
using Logazmic.Core.Readers.Parsers;

namespace Logazmic.Core.Receiver
{
    public class UdpReceiver : ReceiverBase
    {
        private IPEndPoint _remoteEndPoint;

        private UdpClient _udpClient;

        public UdpReceiver()
        {
            BufferSize = 10000;
            Address = string.Empty;
            Port = 7071;
        }

        public int Port { get; set; }

        public bool IpV6 { get; set; }

        public string Address { get; set; }

        public int BufferSize { get; set; }

        #region IReceiver Members

        public override void Terminate()
        {
            _udpClient.Close();
        }

        protected override void DoInitialize()
        {
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            _udpClient = IpV6 ? new UdpClient(Port, AddressFamily.InterNetworkV6) : new UdpClient(Port);
            _udpClient.Client.ReceiveBufferSize = BufferSize;
            if (!string.IsNullOrEmpty(Address))
            {
                _udpClient.JoinMulticastGroup(IPAddress.Parse(Address));
            }
            Task.Factory.StartNew(Start);
        }

        #endregion

        private void Start()
        {
            var logParser = LogReaderFactory.LogParser(LogFormat);

            while (true)
            {
                try
                {
                    byte[] buffer = _udpClient.Receive(ref _remoteEndPoint);
                    string loggingEvent = System.Text.Encoding.UTF8.GetString(buffer);

                    LogMessage logMsg = logParser.TryParseLogEvent(loggingEvent, "UdpLogger");
                    logMsg.LoggerName = string.Format("{0}_{1}", _remoteEndPoint.Address.ToString().Replace(".", "-"), logMsg.LoggerName);
                    OnNewMessage(logMsg);
                }
                catch (SocketException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
            }
        }
    }
}