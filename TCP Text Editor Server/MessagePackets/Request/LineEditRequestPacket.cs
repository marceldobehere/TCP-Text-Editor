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
        public LineInfoBlock Line;

        public LineEditRequestPacket(LineInfoBlock line)
        {
            MessagePacketType = MessagePacketTypeEnum.LINE_EDIT_REQ;
            Line = line;
        }

        public LineEditRequestPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Line = new LineInfoBlock(data);
            //Message = Encoding.ASCII.GetString(data);
        }

        public override byte[] ToByteArray()
        {
            return Line.ToByteArray();
            //List<byte> bytes = new List<byte>();
            //// Convert Message to ASCII and add bytes
            //bytes.AddRange(Encoding.ASCII.GetBytes(Message));
            //return bytes.ToArray();
        }
    }
}
