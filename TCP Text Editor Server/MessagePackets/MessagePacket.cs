﻿using System;
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
        }

        public static Dictionary<MessagePacketTypeEnum, byte> MessagePacketTypeToByte = new Dictionary<MessagePacketTypeEnum, byte>()
        {
            { MessagePacketTypeEnum.NONE, 0 },
            { MessagePacketTypeEnum.ECHO_REQ, 1 },
            { MessagePacketTypeEnum.ECHO_REP, 2 },
            { MessagePacketTypeEnum.LOGIN_REQ, 3 },
            { MessagePacketTypeEnum.LOGIN_REP, 4 },
        };

        public static Dictionary<byte, MessagePacketTypeEnum> ByteToMessagePacketType = new Dictionary<byte, MessagePacketTypeEnum>()
        {
            { 0,MessagePacketTypeEnum.NONE},
            { 1,MessagePacketTypeEnum.ECHO_REQ },
            { 2,MessagePacketTypeEnum.ECHO_REP},
            { 3,MessagePacketTypeEnum.LOGIN_REQ },
            { 4,MessagePacketTypeEnum.LOGIN_REP },
        };

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
