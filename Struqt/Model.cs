using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection;

namespace Qrakhen.Struqt.Models
{
    public abstract class Model
    {
        public string __tableName {
            get { return getTableName(GetType()); }
        }

        protected Definition __def {
            get { return Definition.get(GetType()); }
        }

        protected Database __db {
            get {
                var db = Database.getDatabase(GetType());
                if (db == null) throw new ModelException("model " + GetType().Name + " not registered in any database.");
                else return db;
            }
        }

        protected Type __t {
            get { return GetType(); }
        }

        public virtual void store()
        {
            if ((int)readField(__def.primary.name) == 0 || readField(__def.primary.name) == null) {
                insert();
            } else {
                update();
            }
        }

        protected virtual void insert()
        {
            var q = new Query.Insert(__tableName);
            q.output(__def.primary.column);
            foreach (var field in __def.fields.Values) {
                if (field.primary) continue;
                q.addValue(field.column, readField(field.name));
            }
            var r = __db.scalar(q);
            __t.GetField(__def.primary.name).SetValue(this, r);
        }

        protected virtual void update()
        {
            var q = new Query.Update(
                __tableName, 
                new Where.Equals(
                    __def.primary.column, 
                    readField(__def.primary.name)));
            foreach (var field in __def.fields.Values) {
                if (field.primary) continue;
                q.addValue(field.column, readField(field.name));
            }
            __db.exec(q);
        }

        public virtual void erase()
        {

        }

        protected virtual void readRow(RowReader reader)
        {
            foreach (var field in __def.fields.Values) {
                var f = __t.GetField(field.name);
                if (f == null) throw new ModelDefinitionException("target type does not implement model field " + field.name);
                f.SetValue(this, reader.read(field.column, field.type));
                Console.WriteLine("set " + f.Name + " to " + f.GetValue(this));
            }
        }

        protected object readField(string name)
        {
            try {
                return __t.GetField(name).GetValue(this);
            } catch(Exception e) {
                return null;
            }
        }

        public static List<T> select<T>(Where where)
        {
            var db = Database.getDatabase(typeof(T));
            return db.query<T>(
                new Query.Select(getTableName(typeof(T))).where(where),
                delegate(RowReader reader) {
                    T t = (T)Activator.CreateInstance(typeof(T));
                    (t as Model).readRow(reader);
                    return t;
                });
        }

        public static object getByPrimary(object value, Type model)
        {
            var db = Database.getDatabase(model);
            var result = db.query(
                new Query.Plain("SELECT * FROM @__table")
                    .addArgument("__table", getTableName(model))
                    .where(new Where.Equals(Definition.get(model).primary.column, value)),
                delegate (RowReader reader) {
                    object t = Activator.CreateInstance(model);
                    (t as Model).readRow(reader);
                    return t;
                });
            if (result.Count > 0) return result[0];
            else return null;
        }

        public static T getByPrimary<T>(object value, Type model)
        {
            var db = Database.getDatabase(model);
            var result = db.query<T>(
                new Query.Plain("SELECT * FROM " + getTableName(typeof(T)) + " WHERE " + Definition.get(model).primary.column + "=" + value + ";"),
                delegate (RowReader reader) {
                    T t = (T)Activator.CreateInstance(model);
                    (t as Model).readRow(reader);
                    return t;
                });
            if (result.Count > 0) return result[0];
            else return default(T);
        }

        public T getByPrimary<T>(object value)
        {
            return getByPrimary<T>(value, __t);
        }

        public static void createTable(Type model)
        {
            var def = Definition.get(model);
            string primary = null;
            string sql = "CREATE TABLE " + def.tableName + "(";
            foreach (var field in def.fields.Values) {
                if (field.primary)
                    if (primary == null) primary = field.column;
                    else throw new ModelDefinitionException("multiple primary keys defined in " + def.tableName);
                sql += field.column + " ";
                sql += Field.getSqlType(field.type) + " ";
                sql += field.nullable ? "NULL" : "NOT NULL";
                if (field.increment) sql += " IDENTITY(1,1)";
                sql += ",";
            }
            if (primary == null) throw new ModelDefinitionException("no primary key defined for " + def.tableName);
            sql += "primary key (" + primary + ")";
            sql += ");";
            var db = Database.getDatabase(model);
            db.exec(new Query.Plain(sql));
        }

        protected class Definition
        {
            private static Dictionary<Type, Definition> cached = new Dictionary<Type, Definition>();

            public Field primary { get; private set; }
            public Dictionary<string, Field> fields { get; private set; }
            public string tableName { get; private set; }

            public static Definition get(Type type)
            {
                if (cached.ContainsKey(type)) return cached[type];
                var def = new Definition {
                    tableName = getTableName(type),
                    fields = new Dictionary<string, Field>() };
                var fields = Field.getModelFields(type);
                foreach (var field in fields) {
                    def.fields.Add(field.name, field);
                    if (field.primary) {
                        if (def.primary != null) throw new ModelDefinitionException("multiple primary keys defined in " + getTableName(type));
                        def.primary = field;
                    }
                }
                cached.Add(type, def);
                return def;
            }
        }

        protected class Field
        {
            public string name;
            public string column;
            public bool nullable = true;
            public bool primary = false;
            public bool indexed = false;
            public bool increment = false;
            public Type type;

            public Field(FieldInfo field)
            {
                name = field.Name;
                var _column = field.GetCustomAttribute<Column>();
                if (_column.value == null) column = name.ToLower();
                else column = _column.value;

                var _primary = field.GetCustomAttribute<Primary>();
                if (_primary != null) {
                    primary = _primary.value;
                    nullable = false;
                } else {
                    var _nullable = field.GetCustomAttribute<Null>();
                    if (_nullable != null) nullable = _nullable.value;
                }

                var _indexed = field.GetCustomAttribute<Index>();
                if (_indexed != null) indexed = _indexed.value;

                var _increment = field.GetCustomAttribute<AutoIncrement>();
                if (_increment != null) increment = _increment.value;

                type = field.FieldType;
            }

            public static string getSqlType(Type type)
            {
                if (type == typeof(Guid)) return "nvarchar(max)";
                if (type == typeof(int)) return "int";
                if (type == typeof(long)) return "bigint";
                if (type == typeof(short)) return "int";
                if (type == typeof(DateTime)) return "datetime";
                if (type == typeof(string)) return "nvarchar(max)";
                if (type == typeof(bool)) return "bit";
                if (type == typeof(float)) return "float";
                if (type == typeof(decimal)) return "decimal(24,12)";
                return null;
            }

            public static List<Field> getModelFields(Type model)
            {
                List<Field> fields = new List<Field>();
                foreach (var field in model.GetFields()) {
                    if (field.GetCustomAttribute<Column>() == null) continue;
                    fields.Add(new Field(field));
                }
                return fields;
            }
        }

        public static string getTableName(Type model)
        {
            var info = model.GetCustomAttribute<TableName>();
            if (info == null) return model.Name.ToLower();
            else return info.value;
        }
    }

    public class ModelDefinitionException : ModelException
    {
        public ModelDefinitionException(string message) : base(message) { }
    }

    public class ModelException : Exception
    {
        public ModelException(string message) : base(message) { }
    }
}
