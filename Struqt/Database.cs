using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Qrakhen.Struqt.Models
{
    public class Database
    {
        private static Dictionary<Type, Database> modelToDatabase = new Dictionary<Type, Database>();
        public static Database defaultDatabase { get; private set; }
        private string connectionString;
        private string databaseName;

        public Database(string databaseName, string connectionString)
        {
            this.databaseName = databaseName;
            this.connectionString = connectionString;
            if (defaultDatabase == null) setAsDefault();
        }

        public void setAsDefault()
        {
            defaultDatabase = this;
        }


        public void register(Type model)
        {
            modelToDatabase.Add(model, this);
            if (!tableExists(Model.getTableName(model))) Model.createTable(model);
        }

        internal SqlConnection connect()
        {
            return new SqlConnection(connectionString);
        }

        public bool tableExists(string table)
        {
            var q = new Query.Count("INFORMATION_SCHEMA.TABLES");
            q.where(new Where.Equals("TABLE_NAME", table));
            return (count(q) > 0);
        }

        public delegate T ReaderCallback<T>(RowReader reader);

        public List<T> query<T>(Query query, ReaderCallback<T> callback)
        {
            List<T> result = new List<T>();
            using (var sql = connect()) {
                using (var cmd = new SqlCommand(useDatabase(query.build()), sql)) {
                    foreach (var a in query.arguments) cmd.Parameters.AddWithValue(a.Key, a.Value);
                    sql.Open();
                    using (var dr = cmd.ExecuteReader()) {
                        var reader = new RowReader(dr);
                        while (dr.Read()) result.Add(callback(reader));
                    }
                }
            }
            return result;
        }

        public T queryFirst<T>(Query.Select query, ReaderCallback<T> callback)
        {
            query.limit(1);
            var result = query<T>(query, callback);
            if (result.Count == 0) return default(T);
            else return result[0];
        }

        public int count(Query.Count query)
        {
            int count = 0;
            using (var sql = connect()) {
                using (var cmd = new SqlCommand(useDatabase(query.build()), sql)) {
                    foreach (var a in query.arguments) cmd.Parameters.AddWithValue(a.Key, a.Value);
                    sql.Open();
                    using (var dr = cmd.ExecuteReader()) {
                        if (dr.Read()) count = dr.GetInt32(dr.GetOrdinal("count"));
                    }
                }
            }
            return count;
        }

        public int exec(Query query)
        {
            int rows = 0;
            using (var sql = connect()) {
                using (var cmd = new SqlCommand(useDatabase(query.build()), sql)) {
                    foreach (var a in query.arguments) cmd.Parameters.AddWithValue(a.Key, a.Value);
                    sql.Open();
                    rows = cmd.ExecuteNonQuery();
                }
            }
            return rows;
        }

        public object scalar(Query.Insert query)
        {
            object result = null;
            using (var sql = connect()) {
                using (var cmd = new SqlCommand(useDatabase(query.build()), sql)) {
                    foreach (var a in query.arguments) cmd.Parameters.AddWithValue(a.Key, a.Value);
                    sql.Open();
                    result = cmd.ExecuteScalar();
                }
            }
            return result;
        }

        private string useDatabase(string str)
        {
            return "USE " + databaseName + "; " + str;
        }

        internal static Database getDatabase(Type model)
        {
            if (modelToDatabase.ContainsKey(model)) return modelToDatabase[model];
            else return null;
        }
    }
}
