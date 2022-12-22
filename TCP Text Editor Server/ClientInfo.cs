using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.MessagePackets;

namespace TCP_Text_Editor_Server
{
    public class ClientInfo
    {
        public Socket ClientSocket;

        public Queue<MessagePacket> Messages = new Queue<MessagePacket>();

        public ClientInfo(Socket socket)
        {
            ClientSocket = socket;
        }


        public bool CheckPackets()
        {
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
            }

            return Messages.Count > 0;
        }

        public void SendPacket(MessagePacket packet)
        {
            byte[] data = MessagePacket.GetByteArrayFromPacket(packet);
            int sent = 0;
            while (sent < data.Length)
                sent += ClientSocket.Send(data, sent, data.Length, SocketFlags.None);
        }
    }
}
