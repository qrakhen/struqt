using System;

namespace Qrakhen.Struqt.Models
{
    /// <summary>
    /// Wheter this column is nullable.
    /// Default: true
    /// </summary>
    public class Null : ColumnAttributeAbstract
    {
        public bool value;

        public Null(bool value)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Defines the primary column.
    /// </summary>
    public class Primary : ColumnAttributeAbstract
    {
        public bool value = true;
    }

    /// <summary>
    /// Indexes the column.
    /// </summary>
    public class Index : ColumnAttributeAbstract
    {
        public bool value = true;
    }

    /// <summary>
    /// Indexes the column.
    /// </summary>
    public class AutoIncrement : ColumnAttributeAbstract
    {
        public bool value = true;
    }

    /// <summary>
    /// Defines this member to be treated as table column.
    /// Also defines the column's name.
    /// Default: member name in lowercase.
    /// </summary>
    public class Column : ColumnAttributeAbstract
    {
        public string value;

        public Column(string value = null)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Defines the table name for this model.
    /// Default: class name in lowercase.
    /// </summary>
    public class TableName : TableAttributeAbstract
    {
        public string value;

        public TableName(string value)
        {
            this.value = value;
        }
    }

    public class TableAttributeAbstract : Attribute
    {
    }

    public class ColumnAttributeAbstract : Attribute
    {
    }
}
