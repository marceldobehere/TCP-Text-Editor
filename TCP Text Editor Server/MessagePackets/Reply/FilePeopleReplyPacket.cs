using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    public class FilePeopleReplyPacket : MessagePacket
    {
        public List<ClientInfo> Clients;

        public FilePeopleReplyPacket(List<ClientInfo> clients)
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_PPL_REP;
            Clients = clients;
        }

        public FilePeopleReplyPacket()
        {
            MessagePacketType = MessagePacketTypeEnum.FILE_PPL_REP;
            Clients = new List<ClientInfo>();
        }

        public FilePeopleReplyPacket(byte[] data)
        {
            FromByteArray(data);
        }

        public override void FromByteArray(byte[] data)
        {
            Clients = new List<ClientInfo>();

            int offset = 0;

            int amt = BitConverter.ToInt32(data, offset);
            offset += 4;
            for (int i = 0; i < amt; i++)
            {
                int amt2 = BitConverter.ToInt32(data, offset);
                offset += 4;
                byte[] temp = new byte[amt2];
                for (int z = 0; z < amt2; z++)
                    temp[z] = data[offset + z];
                offset += amt2;
                Clients.Add(new ClientInfo(temp));
            }
        }

        public override byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            
            bytes.AddRange(BitConverter.GetBytes(Clients.Count));
            for (int i = 0; i < Clients.Count; i++)
            {
                byte[] data = Clients[i].ToPplByteArray();
                bytes.AddRange(BitConverter.GetBytes(data.Length));
                bytes.AddRange(data);
            }
            return bytes.ToArray();
        }
    }
}
