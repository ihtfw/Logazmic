namespace Logazmic.Core.Reciever
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class TcpReceiver : AReceiver
    {
        Socket socket;

        public int Port { get; set; }
        public bool IpV6 { get; set; }
        public int BufferSize { get; set; }

        public TcpReceiver()
        {
            Port = 4505;
            BufferSize = 10000;
        }

        protected override void DoInitilize()
        {
            if (socket != null) return;

            socket = new Socket(IpV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                     {
                         ExclusiveAddressUse = true
                     };
            socket.Bind(new IPEndPoint(IpV6 ? IPAddress.IPv6Any : IPAddress.Any, Port));
            socket.Listen(100);
            socket.ReceiveBufferSize = BufferSize;

            var args = new SocketAsyncEventArgs();
            args.Completed += AcceptAsyncCompleted;

            socket.AcceptAsync(args);
        }

        void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (socket == null || e.SocketError != SocketError.Success) return;
            //This is original from Log2Console source
            new Thread(Start) { IsBackground = true }.Start(e.AcceptSocket);
//            var newSocket = e.AcceptSocket;
//            Task.Factory.StartNew(() => Start(newSocket));

            e.AcceptSocket = null;
            socket.AcceptAsync(e);
        }

        void Start(object newSocket)
        {
            try
            {
                using (var socket = (Socket)newSocket)
                {
                    using (var ns = new NetworkStream(socket, FileAccess.Read, false))
                    {
                        while (this.socket != null)
                        {
                            var logMsg = ReceiverUtils.ParseLog4JXmlLogEvent(ns, "TcpLogger");
                            logMsg.LoggerName = string.Format(":{1}.{0}", logMsg.LoggerName, Port);

                            OnNewMessage(logMsg);
                        }
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void Terminate()
        {
            if (socket == null) return;

            socket.Close();
            socket = null;
        }
    }
}
