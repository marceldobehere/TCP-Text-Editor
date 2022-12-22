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


                    System.Threading.Thread.Sleep(20);
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
            if (packet is EchoRequestPacket)
            {
                EchoRequestPacket ep = (packet as EchoRequestPacket);
                Console.WriteLine($"< Got Echo Req Packet from {client}: \"{ep.Message}\"");
                client.SendPacket(new EchoReplyPacket("GOT: " + ep.Message));
                return;
            }

            if (packet is EchoReplyPacket)
            {
                EchoReplyPacket ep = (packet as EchoReplyPacket);
                Console.WriteLine($"< Got Echo Rep Packet from {client}: \"{ep.Message}\"");
                return;
            }

            if (packet is LoginRequestPacket)
            {
                LoginRequestPacket lp = (packet as LoginRequestPacket);
                Console.WriteLine($"< Got Login Req Packet from {client}: user: \"{lp.Username}\", pass: \"{lp.Password}\"");
                client.SendPacket(new LoginReplyPacket(false)); // never accept logins for now
                return;
            }




            Console.WriteLine($"< Dropped packet {packet} as the server does not know how to handle it!");
        }
    }
}
