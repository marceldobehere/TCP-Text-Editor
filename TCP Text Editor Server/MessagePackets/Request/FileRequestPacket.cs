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

        public int LineHash;

        public List<int> LineHashes;


        public FileRequestPacket(string relativePath, int firstLineNum, int lineCount, int lineHash, List<int> lineHashes)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REQ;
            RelativePath = relativePath;

            FirstLineNumber = firstLineNum;
            UseLineNumber = true;
            LineCount = lineCount;
            LineHash = lineHash;
            LineHashes = lineHashes;
        }

        public FileRequestPacket(string relativePath, ushort firstLineId, int lineCount, int lineHash, List<int> lineHashes)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_REQ;
            RelativePath = relativePath;

            FirstLineId = firstLineId;
            UseLineNumber = false;
            LineCount = lineCount;
            LineHash = lineHash;
            LineHashes = lineHashes;
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
            offset += 1;
            LineHash = BitConverter.ToInt32(data, offset);
            offset += 4;
            int count = BitConverter.ToInt32(data, offset);
            offset += 4;

            LineHashes = new List<int>();
            for (int i = 0; i < count; i++)
            {
                LineHashes.Add(BitConverter.ToInt32(data, offset));
                offset += 4;
            }
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
            bytes.AddRange(BitConverter.GetBytes(LineHash)); // 15 + x

            bytes.AddRange(BitConverter.GetBytes(LineHashes.Count)); // 15 + x
            foreach (int bruh in LineHashes)
                bytes.AddRange(BitConverter.GetBytes(bruh));

            return bytes.ToArray();
        }
    }
}
