using System.Collections.Generic;

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

        public class SubQuery : Where
        {
            public SubQuery(Where where, Query sub) : base(where.column, null) { }

            public override string print()
            {
                string r = column + " IS NULL ";
                return r + base.print();
            }
        }

        public class IsNull : Where
        {
            public IsNull(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + " IS NULL ";
                return r + base.print();
            }
        }

        public class NotNull : Where
        {
            public NotNull(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + " IS NOT NULL ";
                return r + base.print();
            }
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

        public class NotEquals : Where
        {
            public NotEquals(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + "!=@" + column;
                return r + base.print();
            }
        }

        public class Greater : Where
        {
            public Greater(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + ">@" + column;
                return r + base.print();
            }
        }

        public class Smaller : Where
        {
            public Smaller(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + "<@" + column;
                return r + base.print();
            }
        }

        public class GreaterOrEqual : Where
        {
            public GreaterOrEqual(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + ">=@" + column;
                return r + base.print();
            }
        }

        public class SmallerOrEqual : Where
        {
            public SmallerOrEqual(string column, object value) : base(column, value) { }

            public override string print()
            {
                string r = column + "<=@" + column;
                return r + base.print();
            }
        }
    }
}
