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
        public string Message;

        public LoginReplyPacket(bool accepted, string message = "")
        {
            MessagePacketType = MessagePacketTypeEnum.LOGIN_REP;
            Accepted = accepted;
            Message = message;
        }

        public LoginReplyPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Accepted = data[0] == 1;
            byte len1 = data[1];
            Message = Encoding.ASCII.GetString(data, 2, len1);
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)(Accepted ? 1 : 0));

            bytes.Add((byte)Message.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(Message));
            return bytes.ToArray();
        }
    }
}
