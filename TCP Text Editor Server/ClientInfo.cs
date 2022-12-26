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
        public ulong PacketsSent = 0;
        public ulong PacketsReceived = 0;

        public int CursorX = 0, CursorY = 0;

        public string CurrentFile = "";

        public ConsoleColor ClientColor = ConsoleColor.Blue;

        public static Random rnd = new Random();

        public ClientInfo(Socket socket)
        {
            ClientSocket = socket;
            Username = "guest";
            LoggedIn = false;
            CursorX = 0;
            CursorY = 0;
            ClientColor = (ConsoleColor)rnd.Next(16);
        }

        public ClientInfo(byte[] data)
        {
            FromPplByteArray(data);
            LoggedIn = false;
            CurrentFile = "";
        }

        public void UpdateByteCounter(ref ulong bytesSent, ref ulong bytesReceived, ref ulong packetsSent, ref ulong packetsReceived)
        {
            bytesSent += BytesSent;
            BytesSent = 0;
            bytesReceived += BytesReceived;
            BytesReceived = 0;
            
            packetsSent += PacketsSent;
            PacketsSent = 0;
            packetsReceived += PacketsReceived;
            PacketsReceived = 0;
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
                PacketsReceived++;
            }

            return Messages.Count > 0;
        }

        public void SendPacket(MessagePacket packet)
        {
            if (!ClientSocket.IsAlive())
                return;
            PacketsSent++;


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


        public byte[] ToPplByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(CursorX));
            bytes.AddRange(BitConverter.GetBytes(CursorY));
            bytes.Add((byte)ClientColor);
            bytes.AddRange(BitConverter.GetBytes(Username.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(Username));

            return bytes.ToArray();
        }

        public void FromPplByteArray(byte[] bytes)
        {
            int offset = 0;
            CursorX = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            CursorY = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            ClientColor = (ConsoleColor)bytes[offset];
            offset += 1;
            int len = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            Username = Encoding.UTF8.GetString(bytes, offset, len);
        }
    }
}
