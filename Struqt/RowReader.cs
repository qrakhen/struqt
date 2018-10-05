using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qrakhen.Struqt.Models
{
    /// <summary>
    /// Callback that is used to read out rows as used by Database.query(query, callback);
    /// </summary>
    /// <example>
    /// Example Usage:
    /// 
    /// List-Item- list = Database.query-Item-(
    ///     new Query.Select(), 
    ///     delegate(RowReader reader) {
    ///         new Item(
    ///             reader.readInt("id"),
    ///             reader.readString("name"));
    ///         });
    /// </example>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <returns></returns>
    public delegate T RowReaderCallback<T>(RowReader reader);

    /// <summary>
    /// Helper class that is provided together with the 
    /// </summary>
    public class RowReader
    {
        private SqlDataReader dr;

        public RowReader(SqlDataReader dr)
        {
            this.dr = dr;
        }

        public object read(string column, Type fieldType)
        {
            if (fieldType == typeof(DateTime)) return readDateTime(column);
            if (fieldType == typeof(bool)) return readBool(column);
            if (fieldType == typeof(long)) return readLong(column);
            if (fieldType == typeof(int)) return readInt(column);
            if (fieldType == typeof(short)) return readShort(column);
            if (fieldType == typeof(string)) return readString(column);
            if (fieldType == typeof(Guid)) return new Guid(readString(column));
            throw new NotImplementedException("implement " + fieldType.Name + " for fucks sake!");
        }

        public DateTime readDateTime(string column)
        {
            if (!dr.IsDBNull(dr.GetOrdinal(column)))
                return dr.GetDateTime(dr.GetOrdinal(column));
            else
                return DateTime.MinValue;
        }

        public bool readBool(string column, bool fallBack = false)
        {
            if (!dr.IsDBNull(dr.GetOrdinal(column)))
                return dr.GetBoolean(dr.GetOrdinal(column));
            else
                return fallBack;
        }

        public long readLong(string column, long fallBack = 0)
        {
            if (!dr.IsDBNull(dr.GetOrdinal(column)))
                return dr.GetInt64(dr.GetOrdinal(column));
            else
                return fallBack;
        }

        public int readInt(string column, int fallBack = 0)
        {
            if (!dr.IsDBNull(dr.GetOrdinal(column)))
                return dr.GetInt32(dr.GetOrdinal(column));
            else
                return fallBack;
        }

        public int readShort(string column, int fallBack = 0)
        {
            if (!dr.IsDBNull(dr.GetOrdinal(column)))
                return dr.GetInt16(dr.GetOrdinal(column));
            else
                return fallBack;
        }

        public string readString(string column, string fallBack = null)
        {
            if (!dr.IsDBNull(dr.GetOrdinal(column)))
                return dr.GetString(dr.GetOrdinal(column));
            else
                return fallBack;
        }
    }
}
