namespace Sentinel.NLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public class NetworkClientWrapper : IDisposable
    {
        private readonly bool isUdp;

        private readonly UdpClient udpClient;

        private readonly TcpListener tcpListener;

        private NetworkStream activeTcpStream;

        public NetworkClientWrapper(NetworkProtocol protocol, IPEndPoint endPoint, System.Threading.CancellationToken cancellationToken)
        {
            isUdp = protocol == NetworkProtocol.Udp;
            if (isUdp)
            {
                udpClient = new UdpClient { ExclusiveAddressUse = false };
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(endPoint);
            }
            else
            {
                tcpListener = new TcpListener(endPoint);
                tcpListener.Start();
                cancellationToken.Register(() => tcpListener.Stop());
            }
        }

        public byte[] Receive(ref IPEndPoint remoteEndPoint, int receiveTimeout)
        {
            if (isUdp)
            {
                udpClient.Client.ReceiveTimeout = receiveTimeout;
                return udpClient.Receive(ref remoteEndPoint);
            }

            var returnBuffer = new List<byte>(100);

            NetworkStream networkStream = null;

            try
            {
                networkStream = activeTcpStream ?? (activeTcpStream = tcpListener.AcceptTcpClient().SetReceiveTimeout(receiveTimeout).GetStream());

                var buffer = new byte[1];
                while (networkStream.Read(buffer, 0, buffer.Length) != 0)
                {
                    if (buffer[0] == (byte)'\n')
                        break;
                    if (buffer[0] == (byte)'\r')
                        continue;

                    returnBuffer.Add(buffer[0]);
                }
            }
            catch
            {
                activeTcpStream = null;
                networkStream?.Close();
                throw;
            }

            return returnBuffer.ToArray();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            udpClient?.Close();
            activeTcpStream?.Close();
            tcpListener?.Stop();
        }
    }

    internal static class TcpClientExtensions
    {
        public static TcpClient SetReceiveTimeout(this TcpClient tcpClient, int receiveTimeout)
        {
            tcpClient.Client.ReceiveTimeout = receiveTimeout;
            return tcpClient;
        }
    }
}