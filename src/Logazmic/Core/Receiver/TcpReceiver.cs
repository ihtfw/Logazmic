using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Logazmic.Core.Receiver
{
    public class TcpReceiver : ReceiverBase
    {
        private Socket _server;

        public int Port { get; set; }
        public bool IpV6 { get; set; }
        public int BufferSize { get; set; }

        public TcpReceiver()
        {
            Port = 4505;
            BufferSize = 128 * 1024; // 128Kb
        }

        protected override void DoInitialize()
        {
            if (_server != null) return;

            _server = new Socket(IpV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);

            // allow other apps listen the same port
            _server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            _server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            var endpoint = new IPEndPoint(IpV6 ? IPAddress.IPv6Any : IPAddress.Any, Port);
            _server.Bind(endpoint);
            _server.Listen(100);
            _server.ReceiveBufferSize = BufferSize;
            
            var args = new SocketAsyncEventArgs();
            args.Completed += AcceptAsyncCompleted;

            _server.AcceptAsync(args);
        }

        void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_server == null || e.SocketError != SocketError.Success) return;

            //This is original from Log2Console source
            new Thread(Start) { IsBackground = true }.Start(e.AcceptSocket);

            e.AcceptSocket = null;
            _server.AcceptAsync(e);
        }

        void Start(object newSocket)
        {
            try
            {
                var logStreamReader = LogReaderFactory.LogStreamReader(LogFormat);
                logStreamReader.DefaultLogger = "TcpLogger";

                using (var socket = (Socket) newSocket)
                {
                    using (var ns = new NetworkStream(socket, FileAccess.Read, false))
                    {
                        int bytesRead;
                        do
                        {
                            foreach (var logMessage in logStreamReader.NextLogEvents(ns, out bytesRead))
                            {
                                logMessage.LoggerName = string.Format(":{1}.{0}", logMessage.LoggerName, Port);
                                OnNewMessage(logMessage);
                            }
                        } while (_server != null && bytesRead > 0);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void Terminate()
        {
            if (_server == null) return;

            _server.Close();
            _server = null;
        }
    }
}
