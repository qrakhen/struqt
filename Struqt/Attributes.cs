using System;

namespace Qrakhen.Struqt.Models
{
    /// <summary>
    /// Declares this field to be a foreign reference,
    /// to the given field of given Model.
    /// This Attribute is only intended for 1:n references.
    /// If you want to implement an n:m model, 
    /// use the Relation Attribute for the desired model.
    /// </summary>
    public sealed class Reference : ColumnAttributeBase
    {
        public Type model;
        public string field;
        public string container;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">The target model this reference points to</param>
        /// <param name="field">The target field name of referenced model. NOT! the column name</param>
        /// <param name="container">The local container field to store referenced object. Can be null, if item should not be read automatically.</param>
        public Reference(Type model, string field, string container = null)
        {
            this.model = model;
            this.field = field;
            this.container = container;
        }
    }

    public sealed class Relation : ColumnAttributeBase
    {

    }

    /// <summary>
    /// Wheter this column is unique.
    /// Default: true
    /// </summary>
    public sealed class Unique : ColumnAttributeBase
    {
        public bool value = true;
    }

    /// <summary>
    /// Wheter this column is nullable.
    /// Default: true
    /// </summary>
    public class Null : ColumnAttributeBase
    {
        public bool value;

        public Null(bool value)
        {
            this.value = value;
        }
    }

    public sealed class NotNull : Null
    {
        public NotNull() : base(false) { }
    }

    /// <summary>
    /// Defines the primary key column.
    /// Does the same as Column("name"), just saving a few lines, conviently.
    /// </summary>
    public sealed class Primary : ColumnAttributeBase
    {
        public string value;

        public Primary(string value = null)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Indexes the column.
    /// </summary>
    public sealed class Index : ColumnAttributeBase
    {
        public string value;

        public Index(string value = "SELF")
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Indexes the column.
    /// </summary>
    public sealed class AutoIncrement : ColumnAttributeBase
    {
        public bool value = true;
    }

    /// <summary>
    /// Defines this member to be treated as table column.
    /// Also defines the column's name.
    /// Default: member name in lowercase.
    /// </summary>
    public sealed class Column : ColumnAttributeBase
    {
        public string value;

        public Column(string value = null)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// I'm not even sure atm.
    /// </summary>
    public sealed class PolyReference : TableAttributeBase
    {
        public string value;

        public PolyReference(string value)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Wether this table shall be cached during runtime.
    /// Default: false
    /// </summary>
    public sealed class CacheTable : TableAttributeBase
    {
        public bool value;

        public CacheTable(bool value = true)
        {
            this.value = value;
        }
    }

    /// <summary>
    /// Defines the table name for this model.
    /// Default: class name in lowercase.
    /// </summary>
    public sealed class TableName : TableAttributeBase
    {
        public string value;

        public TableName(string value = null)
        {
            this.value = value;
        }
    }

    public abstract class TableAttributeBase : Attribute { }

    public abstract class ColumnAttributeBase : Attribute { }
}
