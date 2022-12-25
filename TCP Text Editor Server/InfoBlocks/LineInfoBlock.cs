using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.InfoBlocks
{
    public class LineInfoBlock
    {
        public ushort Id;
        public int LineNumber;
        public string Data;

        public bool Locked;
        public DateTime LockTime;
        public string LockedBy;

        public LineInfoBlock(string data, ushort id, int lineNumber)
        {
            Id = id;
            Data = data;
            LineNumber = lineNumber;


            Locked = false;
            LockTime = DateTime.Now;
            LockedBy = "";
        }

        public LineInfoBlock(int lineNumber, ushort id)
        {
            Id = id;
            Data = "";
            LineNumber = lineNumber;


            Locked = false;
            LockTime = DateTime.Now;
            LockedBy = "";
        }

        public LineInfoBlock(byte[] bytes)
        {
            FromArray(bytes);
        }

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Id)); // 0
            bytes.AddRange(BitConverter.GetBytes(LineNumber)); // 2
            bytes.Add((byte)(Locked ? 1 : 0)); // 6
            bytes.AddRange(BitConverter.GetBytes(Data.Length)); // 7
            bytes.AddRange(Encoding.ASCII.GetBytes(Data)); // 11
            bytes.AddRange(BitConverter.GetBytes(LockedBy.Length)); // 7
            bytes.AddRange(Encoding.ASCII.GetBytes(LockedBy)); // 11


            return bytes.ToArray();
        }

        public void FromArray(byte[] bytes)
        {
            Id = BitConverter.ToUInt16(bytes, 0);
            LineNumber = BitConverter.ToInt32(bytes, 2);
            Locked = bytes[6] == 1;
            int len1 = BitConverter.ToInt32(bytes, 7);
            Data = Encoding.UTF8.GetString(bytes, 11, len1);
            int len2 = BitConverter.ToInt32(bytes, 11 + len1);
            LockedBy = Encoding.UTF8.GetString(bytes, 15 + len1, len2);
        }
    }
}
