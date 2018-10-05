using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Qrakhen.Struqt.Models
{
    public abstract class Query
    {
        protected string _table = null;
        protected Where _where = null;
        public Dictionary<string, object> arguments;

        public Query()
        {
            arguments = new Dictionary<string, object>();
        }

        public Query(string table)
        {
            _table = table;
            arguments = new Dictionary<string, object>();
        }

        public Query table(string table)
        {
            _table = table;
            return this;
        }

        public Query where(Where where)
        {
            if (where == null) return this;
            _where = where;
            addArgument("@" + where.column, where.value);
            foreach (var w in where.parts) addArgument("@" + w.column, w.value);
            return this;
        }

        public Query addArgument(string key, object value)
        {
            arguments.Add(key, value);
            return this;
        }

        protected string buildWhere()
        {
            if (_where == null) return "";
            return " WHERE " + _where.print();
        }

        public abstract string build();        

        public class Plain : Query
        {
            protected string query;

            public Plain(string query) : base()
            {
                this.query = query;
            }

            public override string build()
            {
                return query + buildWhere();
            }
        }

        public class Select : Query
        {
            protected int _limit = 0;
            protected SortOrder sortOrder = SortOrder.Unspecified;
            protected string sortColumn = null;

            public Select() : base() { }
            public Select(string table) : base(table) { }

            public Select limit(int limit)
            {
                _limit = limit;
                return this;
            }

            public Select sort(string column, SortOrder order)
            {
                sortColumn = column;
                sortOrder = order;
                return this;
            }

            protected string buildOrder()
            {
                if (sortColumn == null) return "";
                return " ORDER BY " + sortColumn + " " + (sortOrder == SortOrder.Ascending ? "ASC" : "DESC");
            }

            public override sealed string build()
            {
                string q = "SELECT ";
                if (_limit > 0) q += "TOP (" + _limit + ")";
                q += " * ";
                q += " FROM " + _table + " ";
                q += buildWhere();
                q += buildOrder();
                return q;
            }
        }

        public class Count : Query
        {
            public Count() : base() { }
            public Count(string table) : base(table) { }

            public override sealed string build()
            {
                string q = "SELECT COUNT(*) AS 'count' ";
                q += " FROM " + _table + " ";
                q += buildWhere();
                return q;
            }
        }

        public class Update : Query
        {
            protected Dictionary<string, object> values = new Dictionary<string, object>();

            public Update(Where where) : base() {
                this.where(where);
            }

            public Update(string table, Where where) : base(table) {
                this.where(where);
            }

            public Update addValue(string column, object value)
            {
                values.Add(column, "@" + column);
                addArgument("@" + column, value);
                return this;
            }

            public override sealed string build()
            {
                if (_where == null) throw new InvalidOperationException("no WHERE conditions for rows to be updated were specified");
                if (values.Count < 1) throw new InvalidOperationException("no values provided to update query");
                string q = "UPDATE " + _table + " SET ";
                foreach (var value in values) {
                    q += "[" + value.Key + "]=" + value.Value + ",";
                }
                q = q.Substring(0, q.Length - 1);
                q += buildWhere();
                return q;
            }
        }

        public class Insert : Query
        {
            protected Dictionary<string, object> values = new Dictionary<string, object>();
            protected string _output = null;

            public Insert() : base() { }
            public Insert(string table) : base(table) { }

            /// <summary>
            /// Defines what column (from the just inserted row) should be returned after the query was executed.
            /// </summary>
            /// <param name="column"></param>
            /// <returns></returns>
            public Insert output(string column)
            {
                _output = column;
                return this;
            }

            public Insert addValue(string column, object value)
            {
                values.Add(column, "@" + column);
                addArgument("@" + column, (value == null ? DBNull.Value : value));
                return this;
            }

            public override sealed string build()
            {
                if (values.Count < 1) throw new InvalidOperationException("no values provided to insert query");
                string q = "INSERT INTO " + _table + " (";
                foreach (var value in values) {
                    q += "[" + value.Key + "],";
                }
                q = q.Substring(0, q.Length - 1);
                q += ") ";
                if (_output != null) q += "OUTPUT INSERTED." + _output + " ";
                q += "VALUES (";
                foreach (var value in values) {
                    q += value.Value + ",";
                }
                q = q.Substring(0, q.Length - 1);
                q += ")";
                return q;
            }
        }

        public class Delete : Query
        {
            public Delete(Where where) : base()
            {
                this.where(where);
            }

            public Delete(string table, Where where) : base(table)
            {
                this.where(where);
            }

            public override sealed string build()
            {
                if (_where == null) throw new InvalidOperationException("no WHERE conditions for rows to be deleted were specified");
                string q = "DELETE FROM " + _table + " ";
                q += buildWhere();
                return q;
            }
        }

        public class Create : Query
        {
            protected Type model;

            public Create(string table, Type model) : base(table)
            {
                this.model = model;
            }

            public sealed override string build()
            {
                var def = Model.Definition.get(model);
                string primary = null;
                string q = "CREATE TABLE " + def.tableName + "(";
                foreach (var field in def.fields.Values) {
                    if (field.primary)
                        if (primary == null) primary = field.column;
                        else throw new ModelDefinitionException("multiple primary keys defined in " + def.tableName);
                    q += field.column + " ";
                    q += Model.Field.getSqlType(field.type) + " ";
                    if (field.unique) q += "UNIQUE ";
                    if (field.indexed != null) q += "INDEX " + field.indexed + " ";
                    q += field.nullable ? "NULL" : "NOT NULL";
                    if (field.increment) q += " IDENTITY(1,1)";
                    q += ",";
                }
                if (primary == null) throw new ModelDefinitionException("no primary key defined for " + def.tableName);
                q += "PRIMARY KEY (" + primary + ")";
                q += ");";
                return q;
            }
        }
    }
}
