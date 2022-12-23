using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class FilePeopleRequestPacket : MessagePacket
    {
        public string RelativePath;
        public int X1;
        public int Y1;
        public int X2;
        public int Y2;

        public int CursorX, CursorY;


        public FilePeopleRequestPacket(string relativePath, int cursorX, int cursorY, int x1, int y1, int x2, int y2)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_PPL_REQ;
            RelativePath = relativePath;

            CursorX = cursorX;
            CursorY = cursorY;

            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }


        public FilePeopleRequestPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            int offset = 0;
            CursorX = BitConverter.ToInt32(data, offset);
            offset += 4;
            CursorY = BitConverter.ToInt32(data, offset);
            offset += 4;

            X1 = BitConverter.ToInt32(data, offset);
            offset += 4;
            Y1 = BitConverter.ToInt32(data, offset);
            offset += 4;
            X2 = BitConverter.ToInt32(data, offset);
            offset += 4;
            Y2 = BitConverter.ToInt32(data, offset);
            offset += 4;

            int len1 = BitConverter.ToInt32(data, offset);
            offset += 4;
            RelativePath = Encoding.ASCII.GetString(data, offset, len1);

        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(CursorX)); // 0
            bytes.AddRange(BitConverter.GetBytes(CursorY)); // 4

            bytes.AddRange(BitConverter.GetBytes(X1)); // 8
            bytes.AddRange(BitConverter.GetBytes(Y1)); // 12
            bytes.AddRange(BitConverter.GetBytes(X2)); // 16
            bytes.AddRange(BitConverter.GetBytes(Y2)); // 20 


            bytes.AddRange(BitConverter.GetBytes(RelativePath.Length)); // 24
            bytes.AddRange(Encoding.ASCII.GetBytes(RelativePath)); // 28
            return bytes.ToArray();
        }
    }
}
