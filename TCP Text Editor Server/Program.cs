using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server...");

            Server server = new Server(@"E:\C SHARP\TCP Text Editor\TCP Text Editor Full Client\bin\Debug\testing");

            server.Start();

            server.Loop();

            server.Stop();


            Console.WriteLine("\n\n\nEnd.");
            Console.ReadLine();
        }
    }
}
