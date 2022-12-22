using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class LoginReplyPacket : MessagePacket
    {
        public bool Accepted;

        public LoginReplyPacket(bool accepted)
        {
            MessagePacketType = MessagePacketTypeEnum.LOGIN_REP;
            Accepted = accepted;
        }

        public LoginReplyPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Accepted = data[0] == 1;
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)(Accepted ? 1 : 0));
            return bytes.ToArray();
        }
    }
}
