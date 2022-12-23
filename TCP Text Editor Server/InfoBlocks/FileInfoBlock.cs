using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.InfoBlocks
{
    public class FileInfoBlock
    {
        public string Filename;
        public string RelativePath;
        public List<LineInfoBlock> Lines;

        public HashSet<ushort> givenIds = new HashSet<ushort>();

        private static Random rnd = new Random();

        public ushort RandomId
        {
            get
            {
                ushort id;
                do
                    id = (ushort)rnd.Next(65000);
                while (givenIds.Contains(id));

                return id;
            }
        }

        public FileInfoBlock(string filename, string relativePath)
        {
            Filename = filename;
            RelativePath = relativePath;
            Lines = new List<LineInfoBlock>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                    Lines.Add(new LineInfoBlock(reader.ReadLine(), RandomId, Lines.Count));
            }
        }
    }
}
