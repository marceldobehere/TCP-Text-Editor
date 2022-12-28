using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.InfoBlocks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class LineAddReplyPacket : MessagePacket
    {
        public bool Accepted;
        public int LineNumber;
        public ushort Id;

        public LineAddReplyPacket(bool accepted, int lineNumber, ushort id)
        {
            MessagePacketType = MessagePacketTypeEnum.LINE_EDIT_REP;
            Accepted = accepted;
            LineNumber = lineNumber;
            Id = id;
        }

        public LineAddReplyPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Accepted = data[0] == 1;
            LineNumber = BitConverter.ToInt32(data, 1);
            Id = BitConverter.ToUInt16(data, 5);
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            // Convert Message to ASCII and add bytes
            bytes.Add((byte)(Accepted ? 1 : 0));
            bytes.AddRange(BitConverter.GetBytes(LineNumber));
            bytes.AddRange(BitConverter.GetBytes(Id));
            return bytes.ToArray();
        }
    }
}
