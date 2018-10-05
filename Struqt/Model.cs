using Qrakhen.Struqt.ExtendedTypes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Qrakhen.Struqt.Models
{
    /// <summary>
    /// Pretty Cool class 'ere.
    /// </summary>
    public abstract class Model
    {
        /// <summary>
        /// (Internal) table name
        /// </summary>
        public string __tbl { get { return getTableName(GetType()); } }

        /// <summary>
        /// Model.Definition, stores all database-relevant data.
        /// </summary>
        protected Definition __def { get { return Definition.get(GetType()); } }

        /// <summary>
        /// I need it way too often
        /// </summary>
        protected Type __t { get { return GetType(); } }

        /// <summary>
        /// Primary field
        /// </summary>
        protected Field __pf { get { return __def.primary; } }

        /// <summary>
        /// Primary field column name
        /// </summary>
        protected string __pfc { get { return __pf.column; } }

        /// <summary>
        /// Primary field member name
        /// </summary>
        protected string __pfn { get { return __pf.name; } }

        /// <summary>
        /// Security bool
        /// </summary>
        private bool __ard = false;

        /// <summary>
        /// Quick access to field reading/writing
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected object this[string field] {
            get { return readField(field); }
            set { writeField(field, value); }
        }

        /// <summary>
        /// Shortcut to this model instance's database object.
        /// </summary>
        protected Database __db {
            get {
                var db = Database.getDatabase(GetType());
                if (db == null) throw new ModelException("model " + GetType().Name + " not registered in any database.");
                else return db;
            }
        }

        /// <summary>
        /// Selects a set of objects from this Model, matching the provided Where-Clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public static List<T> select<T>(Where where)
        {
            var db = Database.getDatabase(typeof(T));
            return db.query(
                new Query.Select(getTableName(typeof(T))).where(where),
                delegate (RowReader reader) {
                    T t = (T)Activator.CreateInstance(typeof(T));
                    (t as Model).readRow(reader);
                    return t;
                });
        }

        /// <summary>
        /// Returns the object that matches this model's primary key with provided value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T getByPrimary<T>(object value)
        {
            var t = typeof(T);
            var def = Definition.get(t);
            var db = Database.getDatabase(t);
            var result = db.queryFirst<T>(
                new Query.Select(getTableName(t)).where(new Where.Equals(def.primary.column, value)),
                delegate (RowReader reader) {
                    T obj = (T)Activator.CreateInstance(t);
                    (obj as Model).readRow(reader);
                    return obj;
                });
            return result;
        }

        /// <summary>
        /// Stores this object in the database.
        /// Calls insert() to create a new entry if primaryField is undefined,
        /// or else, update() will be called to overwrite the existing entry.
        /// </summary>
        public virtual void store()
        {
            if ((int)readField(__def.primary.name) == 0 || readField(__def.primary.name) == null) {
                insert();
            } else {
                if (__ard) update();
                else throw new ModelReadWriteException(this, "you can not update an entry that received its primary key before first write. if you want to do this, remove [AutoIncrement] from the model definition and manage ID counting externaly.");
            }
        }

        /// <summary>
        /// Inserts this object's data into the table as a new row.
        /// </summary>
        protected virtual void insert()
        {
            var q = new Query.Insert(__tbl);
            q.output(__def.primary.column);
            foreach (var field in __def.fields.Values) {
                if (field.primary) continue;
                q.addValue(field.column, RowWriter.write(field.type, readField(field.name)));
            }
            var r = __db.scalar(q);
            __t.GetField(__def.primary.name).SetValue(this, r);
        }

        /// <summary>
        /// Updates the existing entry within the table using the primaryField
        /// </summary>
        protected virtual void update(bool overwritePrimaryKey = false)
        {
            var q = new Query.Update(
                __tbl, 
                new Where.Equals(
                    __def.primary.column, 
                    readField(__def.primary.name)));
            foreach (var field in __def.fields.Values) {
                if (field.primary && !overwritePrimaryKey) continue;
                q.addValue(field.column, RowWriter.write(field.type, readField(field.name)));
            }
        }

        /// <summary>
        /// Deletes this entry from the table
        /// </summary>
        public virtual void erase()
        {
            var q = new Query.Delete(__tbl, new Where.Equals(__pfc, readField(__pfn)));
            __db.exec(q);
        }

        /// <summary>
        /// Reads the data from the currently processed row
        /// </summary>
        /// <param name="reader"></param>
        protected virtual void readRow(RowReader reader)
        {
            __ard = true;
            foreach (var field in __def.fields.Values) {
                var f = __t.GetField(field.name);
                if (f == null) throw new ModelDefinitionException("target type does not implement model field " + field.name);
                writeField(f.Name, reader.read(field.column, field.type));
                var _ref = field.reference;
                if (_ref != null && _ref.container != null) {
                    /* probably the ugliest line of code i made yet */
                    var _obj = typeof(Model).GetMethod("getByPrimary").MakeGenericMethod(_ref.model).Invoke(null, new object[] { readField(field.name) });
                    writeField(_ref.container, _obj);
                }
            }
        }

        /// <summary>
        /// Reads value from field with given string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected object readField(string name)
        {
            return __t.GetField(name).GetValue(this);
        }

        /// <summary>
        /// Writes given value into the field
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void writeField(string name, object value)
        {
            __t.GetField(name).SetValue(this, value);
        }

        /// <summary>
        /// Meh.
        /// </summary>
        /// <param name="model"></param>
        internal static void createTable(Type model)
        {
            var db = Database.getDatabase(model);
            db.exec(new Query.Create(Definition.get(model).tableName, model));
        }

        internal protected sealed class Definition
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

        internal protected sealed class Field
        {
            public string name;
            public string column;
            public Type type;
            public bool nullable = true;
            public bool unique = false;
            public bool primary = false;
            public bool increment = false;
            public string indexed = null;
            public Reference reference = null;

            public Field(FieldInfo field)
            {
                name = field.Name;
                type = field.FieldType;

                var _column = field.GetCustomAttribute<Column>();
                if (_column != null) {
                    if (_column.value == null) column = name.ToLower();
                    else column = _column.value;
                }

                var _primary = field.GetCustomAttribute<Primary>();
                if (_primary != null) {
                    primary = true;
                    if (_primary.value != null) column = _primary.value;
                    else if (column == null) column = name.ToLower();
                    nullable = false;
                } else {
                    var _nullable = field.GetCustomAttribute<Null>();
                    if (_nullable != null) nullable = _nullable.value;
                    else {
                        var _notnullable = field.GetCustomAttribute<NotNull>();
                        if (_notnullable != null) nullable = _notnullable.value;
                    }
                }

                var _unique = field.GetCustomAttribute<Unique>();
                if (_unique != null) unique = _unique.value;

                var _indexed = field.GetCustomAttribute<Index>();
                if (_indexed != null) indexed = _indexed.value;

                var _increment = field.GetCustomAttribute<AutoIncrement>();
                if (_increment != null) increment = _increment.value;

                var _reference = field.GetCustomAttribute<Reference>();
                if (_reference != null) reference = _reference;
            }

            public static string getSqlType(Type type)
            {
                if (type == typeof(bool)) return "bit";
                if (type == typeof(int)) return "int";
                if (type == typeof(long)) return "bigint";
                if (type == typeof(short)) return "int";
                if (type == typeof(float)) return "float";
                if (type == typeof(decimal)) return "decimal(24,12)";
                if (type == typeof(string)) return "nvarchar(max)";
                if (type == typeof(Guid)) return "nvarchar(max)";
                if (type == typeof(DateTime)) return "datetime";
                if (type == typeof(NDateTime)) return "datetime";
                if (type.IsEnum) return "int";
                throw new NotSupportedException("type " + type.Name + " not yet supported as column type. sorry :c");
            }

            public static List<Field> getModelFields(Type model)
            {
                List<Field> fields = new List<Field>();
                foreach (var field in model.GetFields()) {
                    if (field.GetCustomAttribute<Column>() == null &&
                        field.GetCustomAttribute<Primary>() == null) continue;
                    fields.Add(new Field(field));
                }
                return fields;
            }
        }

        /// <summary>
        /// Returns the table name for the provided model type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string getTableName(Type model)
        {
            var info = model.GetCustomAttribute<TableName>();
            if (info == null) return model.Name.ToLower();
            else return info.value;
        }
    }

    public class ModelReadWriteException : ModelException
    {
        public Model entry;

        public ModelReadWriteException(Model entry, string message) : base(message)
        {
            this.entry = entry;
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
