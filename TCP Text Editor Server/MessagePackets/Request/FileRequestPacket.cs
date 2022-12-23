using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class FileRequestPacket : MessagePacket
    {
        public string RelativePath;
        public int FirstLineNumber;
        public ushort FirstLineId;
        public bool UseLineNumber;

        public int LineCount;


        public FileRequestPacket(string relativePath, int firstLineNum, int lineCount)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REQ;
            RelativePath = relativePath;

            FirstLineNumber = firstLineNum;
            UseLineNumber = true;
            LineCount = lineCount;
        }

        public FileRequestPacket(string relativePath, ushort firstLineId, int lineCount)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REQ;
            RelativePath = relativePath;

            FirstLineId = firstLineId;
            UseLineNumber = false;
            LineCount = lineCount;
        }

        public FileRequestPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            byte len1 = data[0];
            RelativePath = Encoding.ASCII.GetString(data, 1, len1);
            int offset = len1 + 1;
            FirstLineNumber = BitConverter.ToInt32(data, offset);
            offset += 4;
            FirstLineId = BitConverter.ToUInt16(data, offset);
            offset += 2;
            LineCount = BitConverter.ToInt32(data, offset);
            offset += 4;
            UseLineNumber = data[offset] == 1;

        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)RelativePath.Length); // 0
            bytes.AddRange(Encoding.ASCII.GetBytes(RelativePath)); // 1
            bytes.AddRange(BitConverter.GetBytes(FirstLineNumber)); // 1 + x
            bytes.AddRange(BitConverter.GetBytes(FirstLineId)); // 5 + x
            bytes.AddRange(BitConverter.GetBytes(LineCount)); // 7 + x
            bytes.Add((byte)(UseLineNumber ? 1 : 0)); // 11 + x
            return bytes.ToArray();
        }
    }
}
