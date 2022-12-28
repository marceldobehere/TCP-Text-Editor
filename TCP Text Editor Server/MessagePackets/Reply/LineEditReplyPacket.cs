using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.InfoBlocks;

namespace TCP_Text_Editor_Server.MessagePackets
{
    //public class LineEditReplyPacket : MessagePacket
    //{
    //    public bool Accepted;
    //    public string Data;

    //    public LineEditReplyPacket(bool accepted, string data)
    //    {
    //        MessagePacketType = MessagePacketTypeEnum.LINE_EDIT_REP;
    //        Accepted = accepted;
    //        Data = data;
    //    }

    //    public LineEditReplyPacket(byte[] data)
    //    {
    //        FromByteArray(data);
    //    }

    //    public override void FromByteArray(byte[] data)
    //    {
    //        Accepted = data[0] == 1;
    //        int len = BitConverter.ToInt32(data, 1);
    //        Data = Encoding.ASCII.GetString(data, 5, len);
    //    }

    //    public override byte[] ToByteArray()
    //    {
    //        List<byte> bytes = new List<byte>();
    //        // Convert Message to ASCII and add bytes
    //        bytes.Add((byte)(Accepted ? 1 : 0));
    //        bytes.AddRange(BitConverter.GetBytes(Data.Length));
    //        bytes.AddRange(Encoding.ASCII.GetBytes(Data));
    //        return bytes.ToArray();
    //    }
    //}
}
