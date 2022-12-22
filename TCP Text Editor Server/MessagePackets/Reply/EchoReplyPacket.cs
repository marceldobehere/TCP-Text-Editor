using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class EchoReplyPacket : MessagePacket
    {
       

        public string Message;

        public EchoReplyPacket(string msg)
        {
            MessagePacketType = MessagePacketTypeEnum.ECHO_REP;
            Message = msg;
        }

        public EchoReplyPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Message = Encoding.ASCII.GetString(data);
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            // Convert Message to ASCII and add bytes
            bytes.AddRange(Encoding.ASCII.GetBytes(Message));
            return bytes.ToArray();
        }
    }
}
