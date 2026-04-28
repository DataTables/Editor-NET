using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTables.DatabaseUtil;

namespace DataTables.DatabaseUtil.Oracle
{
    /// <summary>
    /// Oracle result
    /// </summary>
    class Result : DataTables.Result
    {
        public Result(Database db, System.Data.DataTable dt, Query q)
            : base(db, dt, q) { }

        public override string InsertId()
        {
            // The Query will have set up a
            return _query._stmt.Parameters[":dtvalue"].Value.ToString();
        }
    }
}
