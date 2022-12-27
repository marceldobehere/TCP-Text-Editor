using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.InfoBlocks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class LineEditRequestPacket : MessagePacket
    {
        public List<LineInfoBlock> Lines;

        public LineEditRequestPacket(List<LineInfoBlock> line)
        {
            MessagePacketType = MessagePacketTypeEnum.LINE_EDIT_REQ;
            Lines = line;
        }

        public LineEditRequestPacket(LineInfoBlock line)
        {
            MessagePacketType = MessagePacketTypeEnum.LINE_EDIT_REQ;
            Lines = new List<LineInfoBlock>() { line };
        }

        public LineEditRequestPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            int offset = 0;
            int mCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            Lines = new List<LineInfoBlock>();
            for (int i = 0; i < mCount; i++)
            {
                int tCount = BitConverter.ToInt32(data, offset);
                offset += 4;
                byte[] temp = new byte[tCount];
                for (int x = 0; x < tCount; x++)
                    temp[x] = data[x + offset];
                offset += tCount;
                Lines.Add(new LineInfoBlock(temp));
            }
        }

        public override byte[] ToByteArray()
        {
            int count = Lines.Count;
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(count));
            foreach (LineInfoBlock line in Lines)
            {
                byte[] temp = line.ToByteArray();
                bytes.AddRange(BitConverter.GetBytes(temp.Length));
                bytes.AddRange(temp);
            }
            return bytes.ToArray();
        }
    }
}
