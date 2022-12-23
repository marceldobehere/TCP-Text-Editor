﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using TCP_Text_Editor_Server.MessagePackets;
using TCP_Text_Editor_Server.InfoBlocks;
using System.IO;
using TCP_Text_Editor_Server.Extensions;

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

        public Dictionary<string, int> UsernameLogins { get; }

        public Dictionary<string, FileInfoBlock> Files { get; }

        public ulong TotalBytesSent = 0;
        public ulong TotalBytesReceived = 0;





        public Server(string basePath, string ip = "127.0.0.1", int port = 54545)
        {
            BasePath = basePath;
            ServerIP = IPAddress.Parse(ip);
            ServerPort = port;

            MainServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ServerOn = false;
            Clients = new Dictionary<Socket, ClientInfo>();

            UsernameLogins = new Dictionary<string, int>();
            Files = new Dictionary<string, FileInfoBlock>();
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
                int t = 0;
                while (!Exit && ServerOn)
                {
                    if (t++ > 10)
                        t = 0;

                    if (acceptResult == null)
                        acceptResult = MainServerSocket.BeginAccept(null, null);
                    System.Threading.Thread.Sleep(20);
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

                    for (int i = 0; i < Clients.Count; i++)
                    {
                        Socket socket = Clients.ElementAt(i).Key;
                        if (!socket.IsAlive())
                        {
                            Console.WriteLine($"> {Clients.ElementAt(i).Value} Disconnected!");
                            Clients.Remove(socket);
                            i--;
                        }
                    }

                    foreach (var client in Clients.Values)
                    {
                        try
                        {
                            client.UpdateByteCounter(ref TotalBytesSent, ref TotalBytesReceived);
                            if (client.CheckPackets())
                                HandlePacket(client, client.Messages.Dequeue());
                        }
                        catch (Exception e)
                        {

                        }
                    }


                    System.Threading.Thread.Sleep(20);
                    //Console.WriteLine($"> ");

                    if (t == 0)
                        WriteByteStats();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"SERVER LOOP ERROR: \n{e}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void WriteByteStats()
        {
            Console.Title = $"STATS: REC: {TotalBytesReceived}, SENT: {TotalBytesSent}";
        }


        public void HandlePacket(ClientInfo client, MessagePacket packet)
        {
            #region GUEST
            if (packet == null)
            {
                Console.WriteLine($"< NULL PACKET!");
                return;
            }

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
                if (UsernameLogins.ContainsKey(lp.Username))
                {
                    if (UsernameLogins[lp.Username] == lp.Password.GetHashCode())
                    {
                        client.SendPacket(new LoginReplyPacket(true));
                        client.Username = lp.Username;
                        client.LoggedIn = true;
                    }
                    else
                    {
                        client.SendPacket(new LoginReplyPacket(false, "Incorrect Password!"));
                        client.Username = "";
                        client.LoggedIn = false;
                    }
                    return;
                }
                else
                {
                    if (lp.Username.Length < 3)
                    {
                        client.SendPacket(new LoginReplyPacket(false, "Username must be atleast 3 characters long!"));
                        client.Username = "";
                        client.LoggedIn = false;
                        return;
                    }
                    if (lp.Password.Length < 5)
                    {
                        client.SendPacket(new LoginReplyPacket(false, "Password must be atleast 5 characters long!"));
                        client.Username = "";
                        client.LoggedIn = false;
                        return;
                    }
                    Console.WriteLine($"< Making new User for {lp.Username}");
                    UsernameLogins[lp.Username] = lp.Password.GetHashCode();
                    client.SendPacket(new LoginReplyPacket(true, "Created new User."));
                    client.Username = lp.Username;
                    client.LoggedIn = true;
                    return;
                }
            }

            #endregion

            if (!client.LoggedIn)
            {
                Console.WriteLine($"< Dropped packet {packet} as the client is not logged in yet.");
                return;
            }

            #region USER


            if (packet is FileRequestPacket)
            {


                FileRequestPacket fp = (packet as FileRequestPacket);
                Console.WriteLine($"< Got File Req Packet from {client}: \"{fp.RelativePath}\"");


                if (fp.RelativePath.Contains(".."))
                {
                    client.SendPacket(new FileReplyPacket(false, "The path cannot go out of bounds!"));
                    return;
                }

                string fullPath = BasePath + "/" + fp.RelativePath;

                if (!File.Exists(fullPath))
                {
                    client.SendPacket(new FileReplyPacket(false, "The file does not exist!"));
                    return;
                }

                if (fp.FirstLineNumber < 0)
                {
                    client.SendPacket(new FileReplyPacket(false, "Invalid Starting Line!"));
                    return;
                }

                if (fp.LineCount < 0)
                {
                    client.SendPacket(new FileReplyPacket(false, "Invalid Line count!"));
                    return;
                }

                if (!Files.ContainsKey(fp.RelativePath))
                {
                    Files[fp.RelativePath] = new FileInfoBlock(fullPath, fp.RelativePath);
                }

                {
                    FileInfoBlock file = Files[fp.RelativePath];
                    List<LineInfoBlock> lines = new List<LineInfoBlock>();

                    int firstLineNum = fp.FirstLineNumber;
                    if (!fp.UseLineNumber)
                        firstLineNum = file.Lines.FindIndex((LineInfoBlock x) => { return x.Id == fp.FirstLineId; });

                    if (firstLineNum < 0)
                    {
                        client.SendPacket(new FileReplyPacket(false, "Invalid Starting Line ID!"));
                        return;
                    }

                    for (int i = firstLineNum; i < firstLineNum + fp.LineCount && i < file.Lines.Count; i++)
                        lines.Add(file.Lines[i]);

                    client.SendPacket(new FileReplyPacket(true, lines, file.Lines.Count));
                }


                

                return;
            }

            #endregion USER

            Console.WriteLine($"< Dropped packet {packet} as the server does not know how to handle it!");
        }
    }
}
