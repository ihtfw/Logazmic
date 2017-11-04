namespace Logazmic.Core.Reciever
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Log;

    public class UdpReceiver : ReceiverBase
    {
        private IPEndPoint remoteEndPoint;

        private UdpClient udpClient;

        public UdpReceiver()
        {
            BufferSize = 10000;
            Address = String.Empty;
            Port = 7071;
        }

        public int Port { get; set; }

        public bool IpV6 { get; set; }

        public string Address { get; set; }

        public int BufferSize { get; set; }

        #region IReceiver Members

        public override void Terminate()
        {
            udpClient.Close();
        }

        protected override void DoInitilize()
        {
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient = IpV6 ? new UdpClient(Port, AddressFamily.InterNetworkV6) : new UdpClient(Port);
            udpClient.Client.ReceiveBufferSize = BufferSize;
            if (!String.IsNullOrEmpty(Address))
            {
                udpClient.JoinMulticastGroup(IPAddress.Parse(Address));
            }
            Task.Factory.StartNew(Start);
        }

        #endregion

        private void Start()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = udpClient.Receive(ref remoteEndPoint);
                    string loggingEvent = System.Text.Encoding.UTF8.GetString(buffer);

                    LogMessage logMsg = ReceiverUtils.ParseLog4JXmlLogEvent(loggingEvent, "UdpLogger");
                    logMsg.LoggerName = string.Format("{0}_{1}", remoteEndPoint.Address.ToString().Replace(".", "-"), logMsg.LoggerName);
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