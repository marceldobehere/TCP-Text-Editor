using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.Extensions;
using TCP_Text_Editor_Server.MessagePackets;

namespace TCP_Text_Editor_Server
{
    public class ClientInfo
    {
        public Socket ClientSocket;

        public Queue<MessagePacket> Messages = new Queue<MessagePacket>();

        public string Username;
        public bool LoggedIn;

        public ulong BytesSent = 0;
        public ulong BytesReceived = 0;


        public ClientInfo(Socket socket)
        {
            ClientSocket = socket;
            Username = "guest";
            LoggedIn = false;
        }

        public void UpdateByteCounter(ref ulong bytesSent, ref ulong bytesReceived)
        {
            bytesSent += BytesSent;
            BytesSent = 0;
            bytesReceived += BytesReceived;
            BytesReceived = 0;
        }


        public bool CheckPackets()
        {
            if (!ClientSocket.IsAlive())
                return Messages.Count > 0;

            if (ClientSocket.Available > 0)
            {
                byte[] minData = new byte[5];
                int read = 0;
                while (read < 5)
                    read += ClientSocket.Receive(minData, read, minData.Length, SocketFlags.None);

                byte type = minData[0];
                int size = BitConverter.ToInt32(minData, 1);

                byte[] data = new byte[size];
                read = 0;
                while (read < size)
                    read += ClientSocket.Receive(data, read, size, SocketFlags.None);
                Messages.Enqueue(MessagePacket.GetPacketFromByteArray(data, type, false));
                BytesReceived += (ulong)(minData.Length + data.Length);
            }

            return Messages.Count > 0;
        }

        public void SendPacket(MessagePacket packet)
        {
            if (!ClientSocket.IsAlive())
                return;


            byte[] data = MessagePacket.GetByteArrayFromPacket(packet);
            int sent = 0;
            while (sent < data.Length)
                sent += ClientSocket.Send(data, sent, data.Length, SocketFlags.None);
            BytesSent += (ulong)(data.Length);
        }

        public override string ToString()
        {
            return $"<CLIENT - IP: {(ClientSocket.RemoteEndPoint as IPEndPoint).Address.MapToIPv4()} {(ClientSocket.RemoteEndPoint as IPEndPoint).Port}>";
        }
    }
}
