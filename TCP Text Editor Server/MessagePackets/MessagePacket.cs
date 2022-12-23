using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public abstract class MessagePacket
    {
        public enum MessagePacketTypeEnum
        {
            NONE,
            ECHO_REQ,
            ECHO_REP,
            LOGIN_REQ,
            LOGIN_REP,
            FILE_REQ,
            FILE_REP,
            FILE_PPL_REQ,
            FILE_PPL_REP,
        }

        public static Dictionary<MessagePacketTypeEnum, byte> MessagePacketTypeToByte = new Dictionary<MessagePacketTypeEnum, byte>()
        {
            { MessagePacketTypeEnum.NONE, 0 },
            { MessagePacketTypeEnum.ECHO_REQ, 1 },
            { MessagePacketTypeEnum.ECHO_REP, 2 },
            { MessagePacketTypeEnum.LOGIN_REQ, 3 },
            { MessagePacketTypeEnum.LOGIN_REP, 4 },
            { MessagePacketTypeEnum.FILE_REQ, 5 },
            { MessagePacketTypeEnum.FILE_REP, 6 },
            { MessagePacketTypeEnum.FILE_PPL_REQ, 7 },
            { MessagePacketTypeEnum.FILE_PPL_REP, 8 },
        };

        private static Dictionary<byte, MessagePacketTypeEnum> _ByteToMessagePacketType = new Dictionary<byte, MessagePacketTypeEnum>();
        
        private static bool reverseInit = false;
        public static Dictionary<byte, MessagePacketTypeEnum> ByteToMessagePacketType
        {
            get
            {
                if (!reverseInit)
                {
                    foreach (var x in MessagePacketTypeToByte)
                        _ByteToMessagePacketType.Add(x.Value, x.Key);
                    reverseInit = true;
                }
                return _ByteToMessagePacketType;
            }
        }


        protected MessagePacketTypeEnum MessagePacketType;

        public abstract byte[] ToByteArray();

        public abstract void FromByteArray(byte[] data);

        public static MessagePacket GetPacketFromByteArray(byte[] data, byte type, bool hasHeaderdata = false)
        {
            if (hasHeaderdata)
            {
                List<byte> temp = new List<byte>(data);
                temp.RemoveRange(0, 5);
                data = temp.ToArray();
            }

            MessagePacketTypeEnum msgType = ByteToMessagePacketType[type];
            switch (msgType)
            {
                case MessagePacketTypeEnum.ECHO_REQ:
                    return new EchoRequestPacket(data);
                case MessagePacketTypeEnum.ECHO_REP:
                    return new EchoReplyPacket(data);
                case MessagePacketTypeEnum.LOGIN_REQ:
                    return new LoginRequestPacket(data);
                case MessagePacketTypeEnum.LOGIN_REP:
                    return new LoginReplyPacket(data);
                case MessagePacketTypeEnum.FILE_REQ:
                    return new FileRequestPacket(data);
                case MessagePacketTypeEnum.FILE_REP:
                    return new FileReplyPacket(data);
                case MessagePacketTypeEnum.FILE_PPL_REQ:
                    return new FilePeopleRequestPacket(data);
                case MessagePacketTypeEnum.FILE_PPL_REP:
                    return new FilePeopleReplyPacket(data);

                default:
                    return null;
            }
        }

        public static byte[] GetByteArrayFromPacket(MessagePacket packet)
        {
            List<byte> bytes = new List<byte>(packet.ToByteArray());
            int size = bytes.Count;
            bytes.Insert(0, MessagePacketTypeToByte[packet.MessagePacketType]);
            bytes.InsertRange(1, BitConverter.GetBytes(size));
            return bytes.ToArray();
        }
    }
}
