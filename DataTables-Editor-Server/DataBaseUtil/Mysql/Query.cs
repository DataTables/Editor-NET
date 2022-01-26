using System;
using System.Data.Common;

namespace DataTables.DatabaseUtil.Mysql
{
    /// <summary>
    /// MySQL Query class
    /// </summary>
    public class Query : DataTables.Query
    {
       internal override string[] _identifierLimiter => new[] { "`", "`" };
 
        /// <summary>
        /// Create a query instance specifically for MySQL
        /// </summary>
        /// <param name="db">Host database</param>
        /// <param name="type">Query type</param>
        public Query(Database db, string type)
            : base(db, type)
        {
        }

        /// <summary>
        /// Bind parameters to the SQL statement
        /// </summary>
        /// <param name="sql">SQL command</param>
        protected override void _Prepare(string sql)
        {
            var provider = DbProviderFactories.GetFactory(_db.Adapter());
            var cmd = provider.CreateCommand();

            // For insert commands we want the result to be the insert
            // id
            if (_type == "insert")
            {
                sql += "; SELECT LAST_INSERT_ID() as insert_id;";
            }

            cmd.CommandText = sql;
            cmd.Connection = _db.Conn();
            cmd.Transaction = _db.DbTransaction;

            if (_db.CommandTimeout != -1) {
                cmd.CommandTimeout = _db.CommandTimeout;
            }

            // Bind values
            for (int i = 0, ien = _bindings.Count; i < ien; i++)
            {
                var binding = _bindings[i];

                var param = cmd.CreateParameter();
                param.ParameterName = binding.Name;
                param.Value = binding.Value ?? DBNull.Value;

                if (binding.Type != null)
                {
                    param.DbType = binding.Type;
                }

                cmd.Parameters.Add(param);
            }

            _stmt = cmd;

            _db.DebugInfo(sql, _bindings);
        }

        /// <summary>
        /// Execute the SQL command
        /// </summary>
        /// <returns>Result instance</returns>
        protected override DataTables.Result _Exec()
        {
            var dt = new System.Data.DataTable();

            using (var dr = _stmt.ExecuteReader())
            {
                dt.Load(dr);
            }

            return new Mysql.Result(_db, dt, this);
        }
    }
}
