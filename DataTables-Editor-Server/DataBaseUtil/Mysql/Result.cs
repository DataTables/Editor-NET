
namespace DataTables.DatabaseUtil.Mysql
{
    /// <summary>
    /// MySQL result
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
            if (_dt.Rows.Count > 0 && _dt.Columns.Contains("insert_id"))
            {
                return _dt.Rows[0]["insert_id"].ToString();
            }

            return null;
        }
    }
}
