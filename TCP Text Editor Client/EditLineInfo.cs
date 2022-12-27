using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Text_Editor_Server.InfoBlocks;

namespace TCP_Text_Editor_Client
{
    public class EditLineInfo
    {
        //public bool EditCurrentLine { get; private set; } = false;
        //public ushort EditCurrentLineId { get; private set; } = 0;
        //public int EditCurrentLineNum { get; private set; } = -1;
        //public string EditCurrentLineData { get; private set; } = "";
        //public DateTime EditCurrentLineStart { get; private set; }

        public ushort LineId;
        public int LineNumber;
        public LineInfoBlock Line;
        public DateTime StartTime;

        public EditLineInfo(ushort lineId, int lineNumber, LineInfoBlock line, DateTime startTime)
        {
            LineId = lineId;
            LineNumber = lineNumber;
            Line = line;
            StartTime = startTime;
        }

        public EditLineInfo(ushort lineId, int lineNumber, LineInfoBlock line)
        {
            LineId = lineId;
            LineNumber = lineNumber;
            Line = line;
        }
    }
}
