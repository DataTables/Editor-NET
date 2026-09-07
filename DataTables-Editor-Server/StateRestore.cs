// <copyright>Copyright (c) SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// StateRestore class for operation with the client-side extension of the same name.
// </summary>
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DataTables.EditorUtil;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
#else
using System.Web;
#endif

namespace DataTables
{
    public class StateRestore
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Private properties
		*/
        private string _columnDefault = "defaultState";
        private string _columnId = "id";
        private string _columnName = "name";
        private string _columnPath = "path";
        private string _columnShared = "shared";
        private string _columnState = "state";
        private string _columnTable = "table";
        private string _columnUser = "user";
        private Database _db;
        private DtResponse _result;
        private Dictionary<string, object> _set = new Dictionary<string, object>();
        private string _table = "";
        private string _userId = "";
        private readonly List<WhereCondition> _where = new List<WhereCondition>();

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Public methods
		*/

        /// <summary>
        /// Get the column name for the default state flag
        /// </summary>
        /// <returns>Column name</returns>
        public string columnDefault()
        {
            return _columnDefault;
        }

        /// <summary>
        /// Set the database instance used by this instance
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnDefault(string col)
        {
            _columnDefault = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the table's primary key
        /// </summary>
        /// <returns>Column name</returns>
		public string columnId()
        {
            return _columnId;
        }

        /// <summary>
        /// Set the column name for the table's primary key
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnId(string col)
        {
            _columnId = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the state's name
        /// </summary>
        /// <returns>Column name</returns>
		public string columnName()
        {
            return _columnName;
        }

        /// <summary>
        /// Set the column name for the state's name
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnName(string col)
        {
            _columnName = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the URL (path) of where the state applied
        /// </summary>
        /// <returns>Column name</returns>
		public string columnPath()
        {
            return _columnPath;
        }

        /// <summary>
        /// Set the column name for the URL (path) of where the state applied
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnPath(string col)
        {
            _columnPath = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the shared flag
        /// </summary>
        /// <returns>Column name</returns>
		public string columnShared()
        {
            return _columnShared;
        }

        /// <summary>
        /// Set the column name for the shared flag
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnShared(string col)
        {
            _columnShared = col;
            return this;
        }

        /// <summary>
        /// Get the column name for where the state itself is stored
        /// </summary>
        /// <returns>Column name</returns>
		public string columnState()
        {
            return _columnState;
        }

        /// <summary>
        /// Set the column name for where the state itself is stored
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnState(string col)
        {
            _columnState = col;
            return this;
        }

        /// <summary>
        /// Get the column name for where the name of the host DataTable stored
        /// </summary>
        /// <returns>Column name</returns>
		public string columnTable()
        {
            return _columnTable;
        }

        /// <summary>
        /// Set the column name for where the name of the host DataTable stored
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnTable(string col)
        {
            _columnTable = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the name of the column where the user
		/// identifier is stored.
        /// </summary>
        /// <returns>Column name</returns>
		public string columnUser()
        {
            return _columnUser;
        }

        /// <summary>
        /// Set the column name for the name of the column where the user
		/// identifier is stored.
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore columnUser(string col)
        {
            _columnUser = col;
            return this;
        }

        /// <summary>
        /// Get the data constructed and resulting from this instance being
		/// processed.
        /// </summary>
        /// <returns>The result data</returns>
		public DtResponse data()
        {
            return _result;
        }

        /// <summary>
        /// Get the database instance used by this instance
        /// </summary>
        /// <returns>Database connection instance</returns>
        public Database Db()
        {
            return _db;
        }

        /// <summary>
        /// Set the database connection instance
        /// </summary>
        /// <param name="db">Connection instance to set</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Db(Database db)
        {
            _db = db;
            return this;
        }

        public StateRestore Process(StateRestoreRequest data)
        {
            _Process(data);

            return this;
        }

        public StateRestore Process(IEnumerable<KeyValuePair<string, string>> data = null)
        {
            _Process(new StateRestoreRequest(data));

            return this;
        }

#if NETCOREAPP
        /// <summary>
        /// Process the StateRestore quest. For use with WebAPI's 'FormDataCollection' collection
        /// </summary>
        /// <param name="data">Data sent from the client-side</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Process(
            IEnumerable<KeyValuePair<String, StringValues>> data = null,
            string culture = null
        )
        {
            return Process(new StateRestoreRequest(data, culture));
        }
#endif

        /// <summary>
        /// Process a request from the StateRestore client-side to get / set data.
        /// For use with MVC's 'Request.Form' collection
        /// </summary>
        /// <param name="data">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Process(NameValueCollection data = null, string culture = null)
        {
            var list = new List<KeyValuePair<string, string>>();

            if (data != null)
            {
                foreach (var key in data.AllKeys)
                {
                    list.Add(new KeyValuePair<string, string>(key, data[key]));
                }
            }

            return Process(new StateRestoreRequest(list, culture));
        }

        /// <summary>
        /// Process a request from the StateRestore client-side to get / set data.
        /// For use with an HttpRequest object
        /// </summary>
        /// <param name="request">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Process(HttpRequest request, string culture = null)
        {
#if NETCOREAPP
            if (request.HasFormContentType)
            {
                return Process(request.Form);
            }
            else
            {
                var list = new List<KeyValuePair<string, string>>();
                return Process(new StateRestoreRequest(list, culture));
            }
#else
            return Process(request.Form);
#endif
        }

        /// <summary>
        /// Set extra information on the database
        /// </summary>
        /// <param name="column">Column name to write to</param>
        /// <param name="value">Value to write to that column</param>
        /// <returns>Self for chaining</returns>
		public StateRestore Set(string column, string value)
        {
            _set.Add(column, value);
            return this;
        }

        /// <summary>
        /// Get the database table name that will be used for the state storage.
        /// </summary>
        /// <returns>Table name</returns>
        public Database Table()
        {
            return _db;
        }

        /// <summary>
        /// Set the database table name that will be used for state storage.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Table(string table)
        {
            _table = table;
            return this;
        }

        /// <summary>
        // /Get the value for the current user identifier. This is usually a
        /// user id, but it could be any other unique identifier.
        /// </summary>
        /// <returns>User id</returns>
        public string User()
        {
            return _userId;
        }

        /// <summary>
        /// Set the value for the current user identifier. This is usually a
		/// user id, but it could be any other unique identifier.
        /// </summary>
        /// <param name="user">User id</param>
        /// <returns>Self for chaining</returns>
        public StateRestore User(string user)
        {
            _userId = user;
            return this;
        }


        /// <summary>
        /// Where condition to add to the query used to get data from the database.
        /// Multiple conditions can be added if required.
        /// </summary>
        /// <param name="fn">Delegate to execute adding where conditions to the table</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Where(Action<Query> fn)
        {
            _where.Add(new WhereCondition { Custom = fn });

            return this;
        }

        /// <summary>
        /// Where condition to add to the query used to get data from the database.
        /// Multiple conditions can be added if required.
        /// </summary>
        /// <param name="key">Database column name to perform the condition on</param>
        /// <param name="value">Value to use for the condition</param>
        /// <param name="op">Conditional operator</param>
        /// <returns>Self for chaining</returns>
        public StateRestore Where(string key, object value, string op = "=")
        {
            _where.Add(
                new WhereCondition
                {
                    Key = key,
                    Value = value,
                    Operator = op,
                }
            );

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Constructors
		*/

        /// <summary>
        /// Create a new StateRestore instance
        /// </summary>
        /// <param name="db">An instance of the DataTables Database class that we can use for the DB connection. Can also be set with the <code>Db()</code> method.</param>
        StateRestore(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// Create a new StateRestore instance
        /// </summary>
        /// <param name="db">An instance of the DataTables Database class that we can use for the DB connection. Can also be set with the <code>Db()</code> method.</param>
        /// <param name="table">The table name in the database to read and write information from and to. Can also be set with the <code>Table()</code> method.</param>
        StateRestore(Database db, string table)
        {
            _db = db;
            _table = table;
        }

        /// <summary>
        /// Create a new StateRestore instance
        /// </summary>
        /// <param name="db">An instance of the DataTables Database class that we can use for the DB connection. Can also be set with the <code>Db()</code> method.</param>
        /// <param name="table">The table name in the database to read and write information from and to. Can also be set with the <code>Table()</code> method.</param>
        /// <param name="pkey">Primary key column names in the table given. Can also be set with the <code>ColumnId()</code> method.</param>
		StateRestore(Database db, string table, string pkey)
        {
            _db = db;
            _table = table;
            _columnId = pkey;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Private methods
		*/
        private bool _AssertState(DtRequest data)
        {
            // TODO
            return true;
        }

        private StateRestore _Process(StateRestoreRequest data)
        {
            if (data.Action == "state-read")
            {
                // _Read(data);
            }
            else if (data.Action == "state-create")
            {
                // _Create(data);
            }
            else if (data.Action == "state-edit")
            {
                // _Edit(data);
            }
            else if (data.Action == "state-remove")
            {
                // _Remove(data);
            }

            return this;
        }
    }
}