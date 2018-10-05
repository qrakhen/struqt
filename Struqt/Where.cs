using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Qrakhen.Struqt.Models
{
    public abstract class Where
    {
        public List<Where> parts;
        public string column;
        public object value;

        public Where(string column, object value)
        {
            this.column = column;
            this.value = value;
            parts = new List<Where>();
        }

        public Where and(Where where)
        {
            parts.Add(where);
            return this;
        }

        public virtual string print()
        {
            string r = "";
            foreach (var p in parts) r += " AND " + p.print();
            return r;
        }        

        public new class Equals : Where
        {
            public Equals(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + "=@" + column;
                return r + base.print();
            }
        }
    }
}
