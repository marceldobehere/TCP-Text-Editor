using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.MessagePackets;
//using TCP_Text_Editor_Server;

namespace TCP_Text_Editor_Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("> Creating Client...");
            Client client = new Client();

            Console.WriteLine($"Enter IP: (Press Enter to use local server)");
            string ip = Console.ReadLine();
            int port = 0;
            if (ip.Length == 0)
            {
                ip = "localhost";
                port = 54545;
            }
            else
            {
                Console.WriteLine($"Enter Port: ");
                port = int.Parse(Console.ReadLine());
            }
            
            
            Console.WriteLine("> Connecting Client...");
            client.Connect("6.tcp.ngrok.io", 19842);






            Console.WriteLine($"> Starting Client RenderLoop...");
            System.Threading.Thread.Sleep(500);
            Task clientRenderLoop = new Task(client.RenderLoop);
            clientRenderLoop.Start();
            System.Threading.Thread.Sleep(500);







            Console.WriteLine($"> Starting Client Loop...");
            Task clientLoop = new Task(client.Loop);
            clientLoop.Start();

            System.Threading.Thread.Sleep(200);
            Console.WriteLine();




            //Console.WriteLine("> Sending Echo as Client");
            //client.SendPacket(new EchoRequestPacket("HOI!"));
            //client.SendPacket(new EchoRequestPacket("Tomate 123!"));
            //System.Threading.Thread.Sleep(300);
            //Console.WriteLine();


            //System.Threading.Thread.Sleep(200);
            //Console.WriteLine();

            //Console.WriteLine("> Logging in as Client");
            //client.SendPacket(new LoginRequestPacket("masl", "pass123"));




            //System.Threading.Thread.Sleep(200);
            //Console.WriteLine();


            //Console.WriteLine("> Requesting test.txt");
            //client.SendPacket(new FileRequestPacket("test.txt", 0, 10));



            while (!client.Exit)
                ;

            Console.Clear();
            Console.WriteLine("\n\n\nEnd.");
            Console.ReadLine();
        }
    }
}
