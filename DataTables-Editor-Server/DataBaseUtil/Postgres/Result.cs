using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace DataTables.DatabaseUtil.Postgres
{
    /// <summary>
    /// Postgres result
    /// </summary>
    public class Result : DataTables.Result
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

        /// <summary>
        /// Get the last insert ID from an insert query
        /// </summary>
        /// <returns>Last id</returns>
        override public string InsertId()
        {
            if (_dt.Rows.Count > 0 && _dt.Columns.Contains("dt_pkey"))
            {
                return _dt.Rows[0]["dt_pkey"].ToString();
            }

            return null;
        }
    }
}
