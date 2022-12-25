using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class LineAddRequestPacket : MessagePacket
    {
        public ushort LineId1, LineId2;
        public string LineData1, LineData2;

        public LineAddRequestPacket(ushort lineId1, ushort lineId2, string line1, string line2)
        {
            MessagePacketType = MessagePacketTypeEnum.LINE_ADD_REQ;
            LineId1 = lineId1;
            LineId2 = lineId2;
            LineData1 = line1;
            LineData2 = line2;
        }

        public LineAddRequestPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            LineId1 = BitConverter.ToUInt16(data, 0);
            LineId2 = BitConverter.ToUInt16(data, 2);

            int offset = 4;
            int len;
            len = BitConverter.ToInt32(data, offset);
            offset += 4;
            LineData1 = Encoding.UTF8.GetString(data, offset, len);
            offset += len;

            len = BitConverter.ToInt32(data, offset);
            offset += 4;
            LineData2 = Encoding.UTF8.GetString(data, offset, len);
            offset += len;
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(LineId1));
            bytes.AddRange(BitConverter.GetBytes(LineId2));

            bytes.AddRange(BitConverter.GetBytes(LineData1.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(LineData1));
            bytes.AddRange(BitConverter.GetBytes(LineData2.Length));
            bytes.AddRange(Encoding.UTF8.GetBytes(LineData2));

            return bytes.ToArray();
        }
    }
}
