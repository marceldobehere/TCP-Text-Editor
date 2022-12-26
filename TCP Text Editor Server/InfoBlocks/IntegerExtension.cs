using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.InfoBlocks
{
    public static class IntegerExtension
    {
        public static int CombineHashCode(this Int32 a, int b)
        {
            int hash = 17;
            hash = hash * 31 + a;
            hash = hash * 31 + b;
            return hash;
        }
    }
}
