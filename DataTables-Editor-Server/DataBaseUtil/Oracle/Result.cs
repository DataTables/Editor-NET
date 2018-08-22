using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using DataTables.DatabaseUtil;

namespace DataTables.DatabaseUtil.Oracle
{
    /// <summary>
    /// Oracle result
    /// </summary>
    class Result : DataTables.Result
    {
        public Result(Database db, System.Data.DataTable dt, Query q)
            : base(db, dt, q)
        {
        }

        override public string InsertId()
        {
            // The Query will have set up a 
            return _query._stmt.Parameters[":dtvalue"].Value.ToString();
        }
    }
}
