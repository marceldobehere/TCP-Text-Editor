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
            client.SendPacket(new FileRequestPacket("test.txt", 0, 10));




            System.Threading.Thread.Sleep(500);
            Console.WriteLine("Enter to close...");
            Console.ReadLine();
            client.Close();

            Console.WriteLine("\n\n\nEnd.");
            Console.ReadLine();
        }
    }
}
