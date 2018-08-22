// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Class to define the results from an executed Query
// </summary>
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace DataTables
{
    /// <summary>
    /// Result object given by a <code>Query</code> performed on a database.
    /// 
    /// The typical pattern for using this class is to receive an instance of it as a
    /// result of using the <code>Database</code> and <code>Query</code> class methods
    /// that return a result. This class should not be initialised independently.
    /// 
    /// Note that this is a stub class that a driver will extend and complete as
    /// required for individual database types. Individual drivers could add
    /// additional methods, but this is discouraged to ensure that the API is the
    /// same for all database types.
    /// </summary>
    abstract public class Result
    {
        internal Database _db;
        internal System.Data.DataTable _dt;
        internal Query _query;

        private int _FetchPointer = 0;

        /// <summary>
        /// Create a new result instance. Typically this should only be done by
        /// a <code>Query</code> method.
        /// </summary>
        /// <param name="db">Database connection</param>
        /// <param name="dt">Query results</param>
        /// <param name="q">Query</param>
        protected Result(Database db, System.Data.DataTable dt, Query q)
        {
            _db = db;
            _dt = dt;
            _query = q;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Count the number of rows in the result set.
        /// </summary>
        /// <returns>Number of rows in the result set</returns>
        virtual public int Count()
        {
            return _dt.Rows.Count;
        }

        /// <summary>
        /// Get the next row in a result set
        /// </summary>
        /// <returns>Next row</returns>
        virtual public Dictionary<string, object> Fetch()
        {
            if (_FetchPointer < _dt.Rows.Count)
            {
                DataRow row = _dt.Rows[_FetchPointer];
                Dictionary<string, object> data = row.Table.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => row.Field<object>(col.ColumnName));

                _FetchPointer++;

                return data;
            }
            return null;
        }

        /// <summary>
        /// Get all rows in the result set
        /// </summary>
        /// <returns>The data for all rows</returns>
        virtual public List<Dictionary<string, object>> FetchAll()
        {
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

            foreach (DataRow row in _dt.Rows)
            {
                data.Add(
                    row.Table.Columns
                      .Cast<DataColumn>()
                      .ToDictionary(col => col.ColumnName, col => row.Field<object>(col.ColumnName))
                );
            }

            return data;
        }

        /// <summary>
        /// After an INSERT query, get the ID that was inserted.
        /// </summary>
        /// <returns>Insert id</returns>
        virtual public string InsertId()
        {
            return null;
        }

        /// <summary>
        /// Get a DataTable of the results
        /// </summary>
        /// <returns>DataTable filled form the query result</returns>
        public System.Data.DataTable DataTable()
        {
            return _dt;
        }
    }
}
