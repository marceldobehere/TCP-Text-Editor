using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.Extensions
{
    public class ConsoleLogHelper
    {
        public static Queue<string> Backlog { get; private set; } = new Queue<string>();

        public static void WriteLine(string msg)
        {
            Backlog.Enqueue(msg + "\n");
        }

        public static void Write(string msg)
        {
            Backlog.Enqueue(msg);
        }

        public static bool Print = true;

        public static void Loop()
        {
            int logSize = 0;
            int lastLogSize = 0;
            Stopwatch printReset = new Stopwatch();
            while (true)
            {
                if (Backlog.Count > 0)
                {
                    if (Print)
                    {
                        logSize += Backlog.Count;
                        lastLogSize = Backlog.Count;
                        string msg = "";
                        while (Backlog.Count > 0)
                            msg += Backlog.Dequeue();
                        Console.Write(msg);
                    }
                    else
                        Backlog.Clear();
                }
                else
                    logSize = 0;

                if (logSize >= 3)
                {
                    Console.Title = $"Backlog: {lastLogSize}";
                    Print = false;
                    printReset.Start();
                }

                if (printReset.ElapsedMilliseconds > 5000)
                {
                    printReset.Reset();
                    Print = true;
                }
            }
        }

        public static void StartLoop()
        {
            Task bruh = new Task(Loop);
            bruh.Start();
        }

    }
}
