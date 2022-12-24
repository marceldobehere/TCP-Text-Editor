using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server;
using TCP_Text_Editor_Client;
using TCP_Text_Editor_Server.MessagePackets;
using System.Runtime.InteropServices;

namespace TCP_Text_Editor_Full_Client
{
    internal class Program
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        static void Main(string[] args)
        {
            Console.WriteLine("> Starting Server...");

            Server server = new Server("testing");

            server.Start();

            Console.WriteLine($"> Starting Server Loop...");
            Task serverLoop = new Task(server.Loop);
            serverLoop.Start();

            System.Threading.Thread.Sleep(200);
            Console.WriteLine();

            Console.WriteLine("> Creating Client...");
            Client client = new Client();
            Console.WriteLine("> Connecting Client...");
            client.Connect();

            Console.WriteLine($"> Starting Client Loop...");
            Task clientLoop = new Task(client.Loop);
            clientLoop.Start();

            System.Threading.Thread.Sleep(200);
            Console.WriteLine();




            Console.WriteLine("> Sending Echo as Client");
            client.SendPacket(new EchoRequestPacket("HOI!"));
            client.SendPacket(new EchoRequestPacket("Tomate 123!"));
            System.Threading.Thread.Sleep(300);
            Console.WriteLine();

            Console.WriteLine("> Sending Echo as Server");
            foreach (var x in server.Clients)
                x.Value.SendPacket(new EchoRequestPacket("LOL"));
            System.Threading.Thread.Sleep(300);
            Console.WriteLine();


            System.Threading.Thread.Sleep(200);
            Console.WriteLine();

            Console.WriteLine("> Logging in as Client");
            client.SendPacket(new LoginRequestPacket("masl", "pass123"));
            //client.SendPacket(new LoginRequestPacket("masl", "pass123"));
            //client.SendPacket(new LoginRequestPacket("ma", "pass123"));
            //client.SendPacket(new LoginRequestPacket("masl", "pass12"));
            //client.SendPacket(new LoginRequestPacket("masl", "pass123"));



            System.Threading.Thread.Sleep(200);
            Console.WriteLine();


            Console.WriteLine("> Requesting test.txt");
            client.SendPacket(new FileRequestPacket("test.txt", 0, 30));


            System.Threading.Thread.Sleep(600);
            Console.WriteLine();



            Console.WriteLine();
            Console.WriteLine("Enter to stop...");
            Console.ReadLine();
            client.Close();
            server.Stop();



            Console.WriteLine("\n\n\nEnd.");
            Console.ReadLine();
        }
    }
}
