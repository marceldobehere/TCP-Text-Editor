using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.MessagePackets;

namespace TCP_Text_Editor_Client
{
    public class Client
    {
        public IPAddress ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public EndPoint ServerEndPoint { get; private set; }

        public Socket MainSocket { get; }

        public Queue<MessagePacket> Messages = new Queue<MessagePacket>();


        public Client()
        {
            MainSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip = "127.0.0.1", int port = 54545)
        {
            Connect(IPAddress.Parse(ip), port);
        }

        public void Connect(IPAddress serverIP, int serverPort)
        {
            ServerIP = serverIP;
            ServerPort = serverPort;
            ServerEndPoint = new IPEndPoint(ServerIP, ServerPort);
            MainSocket.Connect(ServerEndPoint);
        }

        public void Close()
        {
            MainSocket.Shutdown(SocketShutdown.Both);
        }

        public bool Exit { get; set; }
        
        public void Loop()
        {
            Exit = false;
            while (!Exit)
            {
                if (CheckPackets())
                    HandlePacket(Messages.Dequeue());


                System.Threading.Thread.Sleep(100);
                //Console.WriteLine($"> ");
            }
        }


        public void SendPacket(MessagePacket packet)
        {
            byte[] data = MessagePacket.GetByteArrayFromPacket(packet);
            MainSocket.Send(data);
        }

        public bool CheckPackets()
        {
            if (MainSocket.Available > 0)
            {
                byte[] minData = new byte[5];
                int read = 0;
                while (read < 5)
                    read += MainSocket.Receive(minData, read, minData.Length, SocketFlags.None);

                byte type = minData[0];
                int size = BitConverter.ToInt32(minData, 1);
                byte[] data = new byte[size];
                read = 0;
                while (read < size)
                    read += MainSocket.Receive(data, read, size, SocketFlags.None);
                Messages.Enqueue(MessagePacket.GetPacketFromByteArray(data, type, false));
            }

            return Messages.Count > 0;
        }

        public void HandlePacket(MessagePacket packet)
        {
            if (packet is EchoPacket)
            {
                EchoPacket ep = (packet as EchoPacket);
                Console.WriteLine($"< Got Echo Packet from Server: \"{ep.Message}\"");
            }
        }
    }
}
