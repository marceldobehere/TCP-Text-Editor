using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Text_Editor_Server.Extensions
{
    public static class ListExtensions
    {
        public static int GetSequenceHashCode<T>(this IList<T> sequence)
        {
            const int seed = 487;
            const int modifier = 31;

            unchecked
            {
                return sequence.Aggregate(seed, (current, item) =>
                    (current * modifier) + item.GetHashCode());
            }
        }
    }
}
