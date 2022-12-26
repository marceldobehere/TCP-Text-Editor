using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server;
using TCP_Text_Editor_Server.InfoBlocks;
using TCP_Text_Editor_Server.MessagePackets;
using static TCP_Text_Editor_Client.ConsoleHelper;

namespace TCP_Text_Editor_Client
{
    public class Client
    {
        public IPAddress ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public EndPoint ServerEndPoint { get; private set; }

        public Socket MainSocket { get; }

        public Queue<MessagePacket> Messages = new Queue<MessagePacket>();

        SafeFileHandle _SafeFileHandle;
        CharInfo[] _InternalFullScreenBuffer;
        SmallRect _FullScreenRect;
        CharThing[,] _FullScreenBuffer;

        public List<LineInfoBlock> Lines { get; private set; }

        public string CurrentFile { get; private set; } = "";
        public bool LoggedIn { get; private set; } = false;
        private bool _LoggedInPacket;

        public string Username { get; private set; } = "";

        public List<ClientInfo> PeopleInFile { get; private set; } = new List<ClientInfo>();

        public bool InsertMode { get; private set; } = true;


        public Client()
        {
            MainSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Lines = new List<LineInfoBlock>();


            _OldHeight = Console.WindowHeight;
            _OldWidth = Console.WindowWidth;

            _SafeFileHandle = ConsoleHelper.CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            _InternalFullScreenBuffer = new CharInfo[_OldHeight * _OldWidth];
            _FullScreenRect = new SmallRect() { Left = 0, Right = (short)_OldWidth, Top = 0, Bottom = (short)_OldHeight };
            _FullScreenBuffer = new CharThing[_OldWidth, _OldHeight];
        }

        public void Connect(string ip = "localhost", int port = 54545)
        {
            Connect(Dns.GetHostAddresses(ip)[0], port);
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
        public bool DoRender { get; private set; } = true;

        public bool EditCurrentLine { get; private set; } = false;
        public ushort EditCurrentLineId { get; private set; } = 0;
        public int EditCurrentLineNum { get; private set; } = -1;
        public string EditCurrentLineData { get; private set; } = "";
        public DateTime EditCurrentLineStart { get; private set; }

        public void Loop()
        {
            Exit = false;
            while (!Exit)
            {
                if (CheckPackets())
                    HandlePacket(Messages.Dequeue());


                System.Threading.Thread.Sleep(50);
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
            if (packet is EchoRequestPacket)
            {
                EchoRequestPacket ep = (packet as EchoRequestPacket);
                //Console.WriteLine($"# Got Echo Req Packet from Server: \"{ep.Message}\"");
                SendPacket(new EchoReplyPacket("GOT: " + ep.Message));
                return;
            }

            if (packet is EchoReplyPacket)
            {
                EchoReplyPacket ep = (packet as EchoReplyPacket);
                //Console.WriteLine($"# Got Echo Rep Packet from Server: \"{ep.Message}\"");
                return;
            }

            if (packet is LoginReplyPacket)
            {
                LoginReplyPacket ep = (packet as LoginReplyPacket);
                //Console.WriteLine($"# Got Login Rep Packet from Server: Login Accepted: \"{ep.Accepted}\", Message: \"{ep.Message}\"");

                LoggedIn = ep.Accepted;
                _LoggedInPacket = true;
                return;
            }

            if (packet is FileReplyPacket)
            {
                FileReplyPacket fp = (packet as FileReplyPacket);
                //Console.WriteLine($"# Got File Rep Packet from Server: Accepted: {fp.Accepted}, Message: \"{fp.Message}\", Total Line Count: {fp.TotalLineCount}");
                //Console.WriteLine("# Lines:");
                //foreach (var x in fp.Lines)
                //    Console.WriteLine($" - {x.LineNumber}/{x.Id} - \"{x.Data}\"");
                //Console.WriteLine();

                while (Lines.Count < fp.TotalLineCount)
                    Lines.Add(new LineInfoBlock(Lines.Count, 1));

                foreach (var x in fp.Lines)
                {
                    if (EditCurrentLineId == x.Id && EditCurrentLineNum == x.LineNumber)
                    {
                        if (EditCurrentLine)
                            EditCurrentLine = EditCurrentLineStart.AddSeconds(2) > DateTime.Now;

                        if (!EditCurrentLine)
                            Lines[x.LineNumber] = x;
                    }
                    else
                        Lines[x.LineNumber] = x;
                }

                //if (UseLineId)
                //{
                //    LineInfoBlock info = Lines.Find((LineInfoBlock a) => { return a.Id == UsedLineId; });
                //    if (info != null)
                //    {
                //        int diff = info.LineNumber - CursorY;
                //        ScrollY += diff;
                //        CursorY += diff;
                //        UseLineId = false;
                //        UsedLineId = 0;
                //    }
                //}

                if (CursorY < Lines.Count && Lines[CursorY].Id != CursorYId)
                {
                    LineInfoBlock info = Lines.Find((LineInfoBlock a) => { return a.Id == CursorYId; });
                    if (info == null && CursorY == 0)
                        info = Lines[0];
                    if ((info != null)  || (info == null && CursorY == 0))
                    {
                        int diff = info.LineNumber - CursorY;
                        ScrollY += diff;
                        CursorY += diff;
                        CursorYId = info.Id;
                    }
                    else
                    {
                        if (CursorYId == 0 && CursorY < Lines.Count && CursorYIdPlsNot != Lines[CursorY].Id)
                            CursorYId = Lines[CursorY].Id;
                    }

                }
                else
                {
                    if (CursorY >= Lines.Count)
                        if (Lines.Count > 0)
                            CursorY = Lines.Count - 1;
                }

                return;
            }

            if (packet is FilePeopleReplyPacket)
            {
                FilePeopleReplyPacket fp = (packet as FilePeopleReplyPacket);

                PeopleInFile.Clear();
                PeopleInFile.AddRange(fp.Clients);

                return;
            }

            if (packet is LineEditReplyPacket)
            {
                LineEditReplyPacket lp = (packet as LineEditReplyPacket);

                return;
            }

            Console.WriteLine($"# Dropped packet {packet} as the client does not know how to handle it!");

        }





        private bool UseLineId = false;
        private ushort UsedLineId = 0;


        public void RenderLoop()
        {
            try
            {
                Exit = false;
                int frame = 0;
                int fps = 1;

                Stopwatch watchFps = new Stopwatch();
                watchFps.Start();
                Stopwatch watchFileUpdate = new Stopwatch();
                watchFileUpdate.Start();
                Stopwatch watchPplUpdate = new Stopwatch();
                watchPplUpdate.Start();
                Stopwatch watchLineUpdate = new Stopwatch();
                watchLineUpdate.Start();

                while (!Exit)
                {
                    for (int i = 0; i < 20; i++)
                        HandleKeyboard();


                    if (watchLineUpdate.ElapsedMilliseconds > (LoggedIn ? 150 : 1500))
                    {
                        if (EditCurrentLine)
                        {
                            int index = Lines.FindIndex((LineInfoBlock info) => { return info.Id == EditCurrentLineId; });
                            if (index != -1)
                            {
                                Lines[index].Data = EditCurrentLineData;
                                SendPacket(new LineEditRequestPacket(Lines[index]));
                            }

                            EditCurrentLine = EditCurrentLineStart.AddSeconds(1) > DateTime.Now;
                        }
                        watchLineUpdate.Restart();
                    }

                    System.Threading.Thread.Sleep(10);

                    if (watchFileUpdate.ElapsedMilliseconds >= (LoggedIn ? 150 : 1000))
                    {
                        watchFileUpdate.Restart();

                        int lineBuffer = 5;

                        int tY = CursorY - lineBuffer;
                        if (tY < 0)
                            tY = 0;

                        int amount = _OldHeight + lineBuffer + (CursorY - tY);


                        List<LineInfoBlock> temp = new List<LineInfoBlock>();
                        for (int i = tY; i < tY + amount && i < Lines.Count; i++)
                            temp.Add(Lines[i]);
                        int lineHash = LineInfoBlock.GetLinesHash(temp);


                        if (tY == 0 || tY >= Lines.Count)
                        {
                            UseLineId = false;
                            UsedLineId = 0;
                            SendPacket(new FileRequestPacket(CurrentFile, tY, amount, lineHash));
                        }
                        else
                        {
                            UseLineId = true;
                            UsedLineId = Lines[tY].Id;
                            SendPacket(new FileRequestPacket(CurrentFile, Lines[tY].Id, amount, lineHash));
                        }
                    }

                    if (watchPplUpdate.ElapsedMilliseconds >= (LoggedIn ? 150 : 1000))
                    {
                        watchPplUpdate.Restart();

                        SendPacket(new FilePeopleRequestPacket(CurrentFile, CursorX, CursorY, ScrollX, ScrollY, ScrollX + _OldWidth, ScrollY + _OldHeight));

                    }



                    try
                    { Resize(); }
                    catch (Exception e)
                    { Console.Title = $"RESIZE ERROR: {e.Message} {e}"; }

                    try
                    { Render(); }
                    catch (Exception e)
                    { Console.Title = $"RENDER ERROR: {e.Message} {e}"; }


                    frame++;
                    if (watchFps.ElapsedMilliseconds >= 1000)
                    {
                        fps = frame;
                        frame = 0;
                        watchFps.Restart();

                        Console.Title = $"TCP EDITOR CLIENT - {fps} FPS";
                    }



                }
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private int _OldWidth, _OldHeight;


        public void Resize()
        {
            if (_OldHeight == Console.WindowHeight &&
                _OldWidth == Console.WindowWidth)
                return;

            _OldHeight = Console.WindowHeight;
            _OldWidth = Console.WindowWidth;

            _InternalFullScreenBuffer = new CharInfo[_OldHeight * _OldWidth];
            _FullScreenRect = new SmallRect() { Left = 0, Right = (short)_OldWidth, Top = 0, Bottom = (short)_OldHeight };
            _FullScreenBuffer = new CharThing[_OldWidth, _OldHeight];

        }

        static CharThing _EmptyChar = new CharThing(' ');
        static CharThing _LineChar = new CharThing('-', ConsoleColor.DarkGreen);
        public string TitleText { get; private set; } = "TEST";

        public int ScrollX = 0, ScrollY = 0;
        public int CursorX = 0, CursorY = 0;
        public ushort CursorYId = 0, CursorYIdPlsNot = 0;

        private int _ActualCursorX = 0, _ActualCursorY = 0;


        public void Render()
        {
            if (!DoRender)
                return;

            if (LoggedIn)
                TitleText = $"USER: {Username}, FILE: \"{CurrentFile}\", {PeopleInFile.Count} PEOPLE IN FILE";
            else
                TitleText = $"NOT LOGGED IN";

            if (ScrollY < 0)
                ScrollY = 0;
            if (ScrollX < 0)
                ScrollX = 0;

            if (CursorX < 0)
                CursorX = 0;
            if (CursorY < 0)
                CursorY = 0;

            _ActualCursorX = CursorX - ScrollX;
            _ActualCursorY = CursorY - ScrollY;

            if (_ActualCursorX < 0)
                _ActualCursorX = 0;
            if (_ActualCursorX >= _OldWidth)
                _ActualCursorX = _OldWidth - 1;
            if (_ActualCursorY < 0)
                _ActualCursorY = 0;
            if (_ActualCursorY >= _OldHeight)
                _ActualCursorY = _OldHeight - 1;

            int numLen = $"{ScrollY + _OldHeight + 10}".Length;
            int xOffset = 1 + numLen;
            int yOffset = 2;

            while (_ActualCursorX > _OldWidth - 5 - xOffset)
            {
                _ActualCursorX--;
                ScrollX++;
            }
            while (_ActualCursorY > _OldHeight - 5 - yOffset)
            {
                _ActualCursorY--;
                ScrollY++;
            }

            while (_ActualCursorX < 5 + xOffset && ScrollX > 0)
            {
                _ActualCursorX++;
                ScrollX--;
            }
            while (_ActualCursorY < 5 + yOffset && ScrollY > 0)
            {
                _ActualCursorY++;
                ScrollY--;
            }

            for (int x = 0; x < _OldWidth; x++)
            {
                if (x < TitleText.Length)
                    _FullScreenBuffer[x, 0] = new CharThing(TitleText[x], ConsoleColor.Yellow);
                else
                    _FullScreenBuffer[x, 0] = _EmptyChar;

                _FullScreenBuffer[x, 1] = _LineChar;
            }

            for (int y = 0; y < _OldHeight - yOffset; y++)
            {
                string numStr = (ScrollY + y).ToString().PadLeft(numLen, '0');
                for (int x = 0; x < numLen; x++)
                    _FullScreenBuffer[x, y + yOffset] = new CharThing(numStr[x], ConsoleColor.Cyan);
                if (y + ScrollY < Lines.Count)
                {
                    if (Lines[y + ScrollY].Locked)
                    {
                        if (Lines[y + ScrollY].LockedBy == Username)
                            _FullScreenBuffer[numLen, y + yOffset] = new CharThing('-', ConsoleColor.Yellow);
                        else
                            _FullScreenBuffer[numLen, y + yOffset] = new CharThing('-', ConsoleColor.Red);
                    }
                    else
                        _FullScreenBuffer[numLen, y + yOffset] = new CharThing('-', ConsoleColor.Green);
                }
                else
                    _FullScreenBuffer[numLen, y + yOffset] = new CharThing('-', ConsoleColor.Green);
            }


            for (int y = 0; y < _OldHeight - yOffset; y++)
            {
                if (y + ScrollY < Lines.Count)
                {
                    LineInfoBlock line = Lines[y + ScrollY];
                    for (int x = 0; x < _OldWidth - xOffset; x++)
                    {
                        if (x + ScrollX < line.Data.Length)
                            _FullScreenBuffer[x + xOffset, y + yOffset] = new CharThing(line.Data[x + ScrollX]);
                        else
                            _FullScreenBuffer[x + xOffset, y + yOffset] = _EmptyChar;

                        if (x + ScrollX == CursorX &&
                            y + ScrollY == CursorY)
                        {
                            _ActualCursorX = x + xOffset;
                            _ActualCursorY = y + yOffset;
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < _OldWidth - xOffset; x++)
                    {
                        _FullScreenBuffer[x + xOffset, y + yOffset] = _EmptyChar;

                        if (x + ScrollX == CursorX &&
                            y + ScrollY == CursorY)
                        {
                            _ActualCursorX = x + xOffset;
                            _ActualCursorY = y + yOffset;
                        }
                    }
                }
            }

            foreach (var person in PeopleInFile)
            {
                if (person.CursorX >= ScrollX &&
                    person.CursorY >= ScrollY &&
                    person.CursorX < ScrollX + _OldWidth - xOffset &&
                    person.CursorY < ScrollY + _OldHeight - yOffset)
                {
                    _FullScreenBuffer[person.CursorX - ScrollX + xOffset, person.CursorY - ScrollY + yOffset] = new CharThing('#', person.ClientColor);
                }
            }



            for (int y = 0; y < _OldHeight; y++)
                for (int x = 0; x < _OldWidth; x++)
                    _InternalFullScreenBuffer[x + y * _OldWidth] = _FullScreenBuffer[x, y].ToCharInfo();


            WriteConsoleOutputW(_SafeFileHandle, _InternalFullScreenBuffer, new Coord((short)_OldWidth, (short)_OldHeight), new Coord(0, 0), ref _FullScreenRect);

            Console.CursorLeft = _ActualCursorX;
            Console.CursorTop = _ActualCursorY;

            try
            {
                Console.SetWindowPosition(0, 0);
            }
            catch (Exception e)
            {

            }
        }

        public void HandleKeyboard()
        {
            if (!Console.KeyAvailable)
                return;
            ConsoleKeyInfo info = Console.ReadKey(true);

            if (info.Key == ConsoleKey.LeftArrow)
            {
                if (CursorX > 0)
                    CursorX--;
            }
            else if (info.Key == ConsoleKey.UpArrow)
            {
                if (CursorY > 0)
                    CursorY--;

                if (CursorY < Lines.Count)
                    CursorYId = Lines[CursorY].Id;
            }
            else if (info.Key == ConsoleKey.RightArrow)
            {
                if (CursorX < 1000)
                    CursorX++;
            }
            else if (info.Key == ConsoleKey.DownArrow)
            {
                if (CursorY < Lines.Count + _OldHeight)
                    CursorY++;

                if (CursorY < Lines.Count)
                    CursorYId = Lines[CursorY].Id;
            }
            else if (info.Key == ConsoleKey.Escape)
            {
                Console.Clear();
                Console.WriteLine("> Enter Command:");
                string cmd = Console.ReadLine();
                DoCmd(cmd);
            }



            else if (info.Key == ConsoleKey.Backspace)
            {
                if (CursorY >= 0 && CursorY < Lines.Count)
                {
                    if (CursorX > 0)
                    {
                        if (CursorX < Lines[CursorY].Data.Length + 1)
                        {
                            if (!Lines[CursorY].Locked || Lines[CursorY].LockedBy == Username)
                            {
                                Lines[CursorY].Data =
                                Lines[CursorY].Data.Substring(0, CursorX - 1) +
                                Lines[CursorY].Data.Substring(CursorX, Lines[CursorY].Data.Length - CursorX);

                                EditCurrentLine = true;
                                EditCurrentLineStart = DateTime.Now;
                                EditCurrentLineId = Lines[CursorY].Id;
                                EditCurrentLineData = Lines[CursorY].Data;
                                EditCurrentLineNum = CursorY;
                                Lines[CursorY].LockedBy = Username;
                                Lines[CursorY].Locked = true;
                                //SendPacket(new LineEditRequestPacket(Lines[CursorY]));

                            }

                        }
                        if (!Lines[CursorY].Locked || Lines[CursorY].LockedBy == Username)
                            CursorX--;
                    }
                    else
                    {
                        // DELETE LINE
                    }
                }
            }

            else if (info.Key == ConsoleKey.Enter)
            {
                if (CursorYId != 0)
                {
                    int CursorX2 = CursorX;
                    CursorX = 0;
                    int CursorY2 = CursorY++;

                    if (CursorY2 < Lines.Count)
                        CursorYId = 0;
                    CursorYIdPlsNot = 0;
                    if (CursorY < Lines.Count)
                        CursorYIdPlsNot = Lines[CursorY].Id;

                    // NEW 
                    if (CursorX2 < Lines[CursorY2].Data.Length)
                    {
                        SendPacket(new LineAddRequestPacket(Lines[CursorY2].Id, 0,
                            Lines[CursorY2].Data.Substring(0, CursorX2),
                            Lines[CursorY2].Data.Substring(CursorX2, Lines[CursorY2].Data.Length - CursorX2))
                            );
                    }
                    else
                    {
                        SendPacket(new LineAddRequestPacket(Lines[CursorY2].Id, 0,
                            Lines[CursorY2].Data,
                            "")
                            );
                    }
                }

            }

            else
            {
                if (CursorY >= 0 && CursorY < Lines.Count)
                {
                    //if (CursorX > 0)
                    {
                        if (CursorX < Lines[CursorY].Data.Length + 1)
                        {
                            string n1 = info.KeyChar.ToString().Replace("\t", "    ");
                            int amt = n1.Length;
                            if (!Lines[CursorY].Locked || Lines[CursorY].LockedBy == Username)
                            {
                                if (InsertMode)
                                {
                                    Lines[CursorY].Data =
                                        Lines[CursorY].Data.Substring(0, CursorX) +
                                        n1 +
                                        Lines[CursorY].Data.Substring(CursorX, Lines[CursorY].Data.Length - CursorX);
                                }
                                else
                                {
                                    Lines[CursorY].Data =
                                        Lines[CursorY].Data.Substring(0, CursorX - 1) +
                                        n1 +
                                        Lines[CursorY].Data.Substring(CursorX, Lines[CursorY].Data.Length - CursorX);
                                }
                                //SendPacket(new LineEditRequestPacket(Lines[CursorY]));

                                EditCurrentLine = true;
                                EditCurrentLineStart = DateTime.Now;
                                EditCurrentLineId = Lines[CursorY].Id;
                                EditCurrentLineData = Lines[CursorY].Data;
                                EditCurrentLineNum = CursorY;
                                Lines[CursorY].LockedBy = Username;
                                Lines[CursorY].Locked = true;
                            }
                            CursorX += amt;
                        }
                    }
                }
            }

        }


        public void DoCmd(string cmd)
        {
            if (cmd.Equals("login"))
            {
                LoggedIn = false;
                _LoggedInPacket = false;
                Console.WriteLine("> Enter Username:");
                string username = Console.ReadLine();
                Username = username;
                Console.WriteLine("> Enter Password:");
                string pass = Console.ReadLine();
                Console.WriteLine("\n> Waiting for reply...");
                SendPacket(new LoginRequestPacket(username, pass));
                while (!_LoggedInPacket)
                    ;

                if (!LoggedIn)
                {
                    Console.WriteLine("Not Logged in!");
                    Console.ReadLine();
                }
                else
                    Console.WriteLine("Logged in.");
                return;
            }

            if (cmd.Equals("file"))
            {
                Console.WriteLine("> Enter Filename:");
                string filename = Console.ReadLine();
                CurrentFile = filename;
                Lines.Clear();
                PeopleInFile.Clear();
                CursorX = 0;
                CursorY = 0;
                ScrollX = 0;
                ScrollY = 0;
                return;
            }
        }





    }
}
