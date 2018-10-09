using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace DataTables.DatabaseUtil.Sqlite
{
    /// <summary>
    /// SQLite result
    /// </summary>
    class Result : DataTables.Result
    {
        /// <summary>
        /// Create a result. This will only ever be called by the Query class.
        /// </summary>
        /// <param name="db">Database instance</param>
        /// <param name="dt">DataTable containing the results</param>
        /// <param name="q">Source query</param>
        public Result(Database db, System.Data.DataTable dt, Query q)
            : base(db, dt, q)
        {
        }

        override public string InsertId()
        {
            var provider = DbProviderFactories.GetFactory(_db.Adapter());
            var cmd = provider.CreateCommand();

            cmd.CommandText = "select last_insert_rowid()";
            cmd.Connection = _db.Conn();
            cmd.Transaction = _db.DbTransaction;

            var id = cmd.ExecuteScalar();
            return id.ToString();
        }
    }
}
