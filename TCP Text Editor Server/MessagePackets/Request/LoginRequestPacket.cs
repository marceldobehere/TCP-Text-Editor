using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class LoginRequestPacket : MessagePacket
    {

        public string Username, Password;

        public LoginRequestPacket(string username, string password)
        {
            MessagePacketType = MessagePacketTypeEnum.LOGIN_REQ;
            Username = username;
            Password = password;
        }

        public LoginRequestPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            byte len1 = data[0];
            Username = Encoding.ASCII.GetString(data, 1, len1);
            byte len2 = data[len1+1];
            Password = Encoding.ASCII.GetString(data, len1+2, len2);
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            // Convert Message to ASCII and add bytes
            bytes.Add((byte)Username.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(Username));
            bytes.Add((byte)Password.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(Password));
            return bytes.ToArray();
        }
    }
}
