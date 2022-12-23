using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Client
{
    public class ConsoleHelper
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutputW(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public ushort UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        public struct CharThing
        {
            public char Chr;
            public ConsoleColor ForegroundColor, BackgroundColor;
            public bool Underscore;

            public CharInfo ToCharInfo()
            {
                return new CharInfo()
                {
                    Char = new CharUnion() { AsciiChar = (byte)Chr },
                    Attributes = (short)
                    (
                        (byte)ForegroundColor +
                        ((byte)BackgroundColor * 16) +
                        (Underscore ? 0x8000 : 0)
                    )
                };
            }

            public CharThing(char chr, bool underscore = false)
            {
                Chr = chr;
                Underscore = underscore;
                ForegroundColor = ConsoleColor.White;
                BackgroundColor = ConsoleColor.Black;
            }

            public CharThing(char chr, ConsoleColor fg, bool underscore = false)
            {
                Chr = chr;
                Underscore = underscore;
                ForegroundColor = fg;
                BackgroundColor = ConsoleColor.Black;
            }

            public CharThing(char chr, ConsoleColor fg, ConsoleColor bg, bool underscore = false)
            {
                Chr = chr;
                Underscore = underscore;
                ForegroundColor = fg;
                BackgroundColor = bg;
            }
        }


        [STAThread]
        static void Test(string[] args)
        {
            SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (h.IsInvalid)
                return;

            short mult = 1;

            CharInfo[] buf = new CharInfo[80 * 25 * mult * mult];
            SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)(80 * mult), Bottom = (short)(25 * mult) };
            //Console.ReadKey();
            for (byte character = 65; character < 65 + 26; ++character)
            {
                rect.Left = (byte)(character - 65);
                rect.Right = (byte)((80 * mult) + character - 65);
                for (short attribute = 0; attribute < 15; ++attribute)
                {
                    for (int i = 0; i < buf.Length; ++i)
                    {
                        buf[i].Attributes = attribute;
                        buf[i].Char.AsciiChar = character;
                    }

                    WriteConsoleOutputW(h, buf,
                      new Coord() { X = (short)(80 * mult + 10), Y = (short)(25 * mult + 2) },
                      new Coord() { X = 0, Y = 0 },
                      ref rect);
                }
                //Console.ReadKey();
            }

            Console.ReadKey();
        }
    }
}
