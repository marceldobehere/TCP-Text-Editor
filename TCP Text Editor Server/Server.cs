using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using TCP_Text_Editor_Server.MessagePackets;

namespace TCP_Text_Editor_Server
{
    public class Server
    {
        public string BasePath { get; }
        public IPAddress ServerIP { get; }
        public int ServerPort { get; }

        public Socket MainServerSocket { get; private set; }
        public bool ServerOn { get; private set; }

        public Dictionary<Socket, ClientInfo> Clients { get; }
        public bool Exit { get; private set; }


        public Server(string basePath, string ip = "127.0.0.1", int port = 54545)
        {
            BasePath = basePath;
            ServerIP = IPAddress.Parse(ip);
            ServerPort = port;

            MainServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ServerOn = false;
            Clients = new Dictionary<Socket, ClientInfo>();
        }

        public void Start()
        {
            MainServerSocket.Bind(new IPEndPoint(ServerIP, ServerPort));
            MainServerSocket.Listen(100);

            ServerOn = true;
        }

        public void Stop()
        {
            ServerOn = false;
            foreach (var conn in Clients.Keys)
                conn.Close();
            Clients.Clear();
            MainServerSocket.Close();

        }

        public void Loop()
        {
            try
            {
                Exit = false;
                IAsyncResult acceptResult = null;
                while (!Exit && ServerOn)
                {
                    if (acceptResult == null)
                        acceptResult = MainServerSocket.BeginAccept(null, null);
                    System.Threading.Thread.Sleep(50);
                    if (acceptResult != null && acceptResult.IsCompleted)
                    {
                        try
                        {
                            Socket conn = MainServerSocket.EndAccept(acceptResult);
                            acceptResult = null;
                            Console.WriteLine($"> Connection moment!");
                            Clients.Add(conn, new ClientInfo(conn));
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    foreach (var client in Clients.Values)
                    {
                        if (client.CheckPackets())
                            HandlePacket(client, client.Messages.Dequeue());
                    }


                    System.Threading.Thread.Sleep(50);
                    //Console.WriteLine($"> ");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"SERVER LOOP ERROR: \n{e}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }


        public void HandlePacket(ClientInfo client, MessagePacket packet)
        {
            if (packet is EchoPacket)
            {
                EchoPacket ep = (packet as EchoPacket);
                Console.WriteLine($"< Got Echo Packet: \"{ep.Message}\"");
                client.SendPacket(new EchoPacket("LOL: " + ep.Message));
            }
        }
    }
}
