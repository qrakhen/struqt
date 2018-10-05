using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qrakhen.Struqt.ExtendedTypes
{
    public class NDateTime
    {
        public DateTime dt;

        public NDateTime(DateTime dt)
        {
            this.dt = dt;
        }

        public override string ToString()
        {
            return dt.ToString();
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return dt.ToString(format, provider);
        }

        public string ToString(string format)
        {
            return dt.ToString(format);
        }

        public string ToString(IFormatProvider provider)
        {
            return dt.ToString(provider);
        }
    }
}
