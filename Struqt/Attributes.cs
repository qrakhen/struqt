﻿using System;

namespace Qrakhen.Struqt.Models
{
    /// <summary>
    /// Declares this field to be a foreign reference,
    /// to the given field of given Model.
    /// This Attribute is only intended for 1:n references.
    /// If you want to implement an n:m model, 
    /// use the PolyReference Attribute for the desired model.
    /// </summary>
    public class Reference : ColumnAttributeAbstract
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
    /// I'm not even sure atm.
    /// </summary>
    public class PolyReference : TableAttributeAbstract
    {
        public string value;

        public PolyReference(string value)
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
