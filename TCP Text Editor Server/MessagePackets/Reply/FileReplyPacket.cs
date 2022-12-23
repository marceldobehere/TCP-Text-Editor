using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.InfoBlocks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class FileReplyPacket : MessagePacket
    {
        public bool Accepted;
        public string Message;

        public List<LineInfoBlock> Lines;
        public int TotalLineCount;

        public FileReplyPacket(bool accepted, string message, List<LineInfoBlock> lines, int totalLineCount)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REP;
            Accepted = accepted;
            Message = message;
            Lines = lines;
            TotalLineCount = totalLineCount;
        }

        public FileReplyPacket(bool accepted, List<LineInfoBlock> lines, int totalLineCount)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REP;
            Accepted = accepted;
            Message = "";
            Lines = lines;
            TotalLineCount = totalLineCount;
        }

        public FileReplyPacket(bool accepted, string message)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REP;
            Accepted = accepted;
            Message = message;
            Lines = new List<LineInfoBlock>();
            TotalLineCount = 0;
        }

        public FileReplyPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Accepted = data[0] == 1;
            byte len1 = data[1];
            Message = Encoding.ASCII.GetString(data, 2, len1);
            int offset = 2 + len1;
            TotalLineCount = BitConverter.ToInt32(data, offset);
            offset += 4;
            int lineCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            Lines = new List<LineInfoBlock>();
            for (int i = 0; i < lineCount; i++)
            {
                ushort byteCount = BitConverter.ToUInt16(data, offset);
                offset += 2;
                byte[] temp = new byte[byteCount];
                for (int z = 0; z < byteCount; z++)
                    temp[z] = data[z + offset];
                offset += byteCount;
                Lines.Add(new LineInfoBlock(temp));
            }
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();

            bytes.Add((byte)(Accepted ? 1 : 0)); // 0
            bytes.Add((byte)Message.Length); // 1
            bytes.AddRange(Encoding.ASCII.GetBytes(Message)); // 2
            bytes.AddRange(BitConverter.GetBytes(TotalLineCount)); // 2 + x

            bytes.AddRange(BitConverter.GetBytes(Lines.Count)); // 6 + x
            for (int i = 0; i < Lines.Count; i++)
            {
                byte[] data = Lines[i].ToByteArray();
                bytes.AddRange(BitConverter.GetBytes((ushort)data.Length));  // 10 + x + y
                bytes.AddRange(data); // 12 + x + y
            }

            return bytes.ToArray();
        }
    }
}
