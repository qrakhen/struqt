using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Qrakhen.Struqt.Models
{
    public sealed class Database
    {
        private static Dictionary<Type, Database> modelToDatabase = new Dictionary<Type, Database>();
        private string connectionString;
        private string databaseName;

        public Database(string databaseName, string connectionString)
        {
            this.databaseName = databaseName;
            this.connectionString = connectionString;
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

        /// <summary>
        /// Returns wheter given table exists on this database.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool tableExists(string table)
        {
            var q = new Query.Count("INFORMATION_SCHEMA.TABLES");
            q.where(new Where.Equals("TABLE_NAME", table));
            return (count(q) > 0);
        }
        
        private List<T> query<T>(Query query, RowReaderCallback<T> callback, bool firstOnly = false)
        {
            List<T> result = new List<T>();
            using (var sql = connect()) {
                using (var cmd = new SqlCommand(useDatabase(query.build()), sql)) {
                    foreach (var a in query.arguments) cmd.Parameters.AddWithValue(a.Key, a.Value);
                    sql.Open();
                    using (var dr = cmd.ExecuteReader()) {
                        var reader = new RowReader(dr);
                        while (dr.Read()) {
                            result.Add(callback(reader));
                            if (firstOnly) break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns all matched rows according to provided query, 
        /// invoking the callback each row, which is required to return the desired object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public List<T> query<T>(Query query, RowReaderCallback<T> callback)
        {
            return query<T>(query, callback, false);
        }

        /// <summary>
        /// Returns the first matched row according to provided query, 
        /// invoking the callback, which is required to return the desired object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public T getFirst<T>(Query query, RowReaderCallback<T> callback)
        {
            var r = query<T>(query, callback, true);
            if (r.Count == 0) return default(T);
            else return r[0];
        }

        /// <summary>
        /// Returns the first matched row according to provided query, 
        /// invoking the callback, which is required to return the desired object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Model getFirst(Query query, RowReaderCallback<Model> callback)
        {
            return getFirst<Model>(query, callback);
        }

        /// <summary>
        /// Executes the provided count query and returns the (single) count result by reading the COUNT result.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Executes provided query and returns number of affected rows.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Executes provided query and returns the output column which is defined using Query.Insert.output();
        /// This is commonly used for previously undefined IDs, to retrieve the new ID directly, without having to use another call.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
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

        public static Database getDatabase(Type model)
        {
            if (modelToDatabase.ContainsKey(model)) return modelToDatabase[model];
            else return null;
        }
    }
}
