using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qrakhen.Struqt.Models
{
    public class RowWriter
    {
        public RowWriter()
        {

        }

        public static object write(Type fieldType, object value)
        {
            if (fieldType.IsEnum) return (int)value;
            return value;
        }        
    }
}
