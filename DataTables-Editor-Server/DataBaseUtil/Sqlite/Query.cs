using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using DataTables.DatabaseUtil;

namespace DataTables.DatabaseUtil.Sqlite
{
    /// <summary>
    /// SQLite Query class
    /// </summary>
    public class Query : DataTables.Query
    {
        /// <summary>
        /// Create a query instance specifically for SQLite
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
        override protected void _Prepare(string sql)
        {
            DbProviderFactory provider = DbProviderFactories.GetFactory(_db.Adapter());
            DbParameter param;
            DbCommand cmd = provider.CreateCommand();

            cmd.CommandText = sql;
            cmd.Connection = _db.Conn();
            cmd.Transaction = _db.DbTransaction;

            // Bind values
            for (int i = 0, ien = _bindings.Count; i < ien; i++)
            {
                var binding = _bindings[i];

                param = cmd.CreateParameter();
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

            return new Sqlite.Result(_db, dt, this);
        }
    }
}
