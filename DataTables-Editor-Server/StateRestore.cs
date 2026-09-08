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
using System.Runtime.Serialization;
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
        private DtResponse _result = new DtResponse();
        private Dictionary<string, object> _set = new Dictionary<string, object>();
        private string _table = "";
        private string _userId = null;
        private readonly List<WhereCondition> _where = new List<WhereCondition>();

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Public methods
		*/

        /// <summary>
        /// Get the column name for the default state flag
        /// </summary>
        /// <returns>Column name</returns>
        public string ColumnDefault()
        {
            return _columnDefault;
        }

        /// <summary>
        /// Set the database instance used by this instance
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnDefault(string col)
        {
            _columnDefault = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the table's primary key
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnId()
        {
            return _columnId;
        }

        /// <summary>
        /// Set the column name for the table's primary key
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnId(string col)
        {
            _columnId = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the state's name
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnName()
        {
            return _columnName;
        }

        /// <summary>
        /// Set the column name for the state's name
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnName(string col)
        {
            _columnName = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the URL (path) of where the state applied
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnPath()
        {
            return _columnPath;
        }

        /// <summary>
        /// Set the column name for the URL (path) of where the state applied
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnPath(string col)
        {
            _columnPath = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the shared flag
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnShared()
        {
            return _columnShared;
        }

        /// <summary>
        /// Set the column name for the shared flag
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnShared(string col)
        {
            _columnShared = col;
            return this;
        }

        /// <summary>
        /// Get the column name for where the state itself is stored
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnState()
        {
            return _columnState;
        }

        /// <summary>
        /// Set the column name for where the state itself is stored
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnState(string col)
        {
            _columnState = col;
            return this;
        }

        /// <summary>
        /// Get the column name for where the name of the host DataTable stored
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnTable()
        {
            return _columnTable;
        }

        /// <summary>
        /// Set the column name for where the name of the host DataTable stored
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnTable(string col)
        {
            _columnTable = col;
            return this;
        }

        /// <summary>
        /// Get the column name for the name of the column where the user
		/// identifier is stored.
        /// </summary>
        /// <returns>Column name</returns>
		public string ColumnUser()
        {
            return _columnUser;
        }

        /// <summary>
        /// Set the column name for the name of the column where the user
		/// identifier is stored.
        /// </summary>
        /// <param name="col">Column name</param>
        /// <returns>Self for chaining</returns>
		public StateRestore ColumnUser(string col)
        {
            _columnUser = col;
            return this;
        }

        /// <summary>
        /// Get the data constructed and resulting from this instance being
		/// processed.
        /// </summary>
        /// <returns>The result data</returns>
        public DtResponse Data()
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
        /// <param name="table">The table name in the database to read and write information from and to. Can also be set with the <code>Table()</code> method.</param>
        /// <param name="pkey">Primary key column names in the table given. Can also be set with the <code>ColumnId()</code> method.</param>
		public StateRestore(Database db = null, string table = null, string pkey = null)
        {
            if (db != null)
            {
                _db = db;
            }

            if (table != null)
            {
                _table = table;
            }

            if (pkey != null)
            {
                _columnId = pkey;
            }
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Private methods
		*/

        /// <summary>
        /// Validate submitted state data.
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns>`true` if valid</returns>
        private bool _AssertState(StateRestoreRequest data)
        {
            if (data.Name == "")
            {
                _result.error = "Incomplete data - no name";
                return false;
            }

            if (data.State == "")
            {
                _result.error = "Incomplete data - no state";
                return false;
            }

            return _AssertStateHost(data);
        }

        /// <summary>
        /// Check the parameters that are submitted for host table information.
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns>`true` if valid</returns>
        private bool _AssertStateHost(StateRestoreRequest data)
        {
            if (data.Path == "")
            {
                _result.error = "Incomplete data - no path";
                return false;
            }

            if (data.Table == "")
            {
                _result.error = "Incomplete data - no table";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Router for the request - based on the action parameter submitted
        /// </summary>
        /// <param name="data">Submitted data</param>
        /// <returns>Self for chaining</returns>
        private StateRestore _Process(StateRestoreRequest data)
        {
            if (data.Action == "state-read")
            {
                _Read(data);
            }
            else if (data.Action == "state-create")
            {
                _Create(data);
            }
            else if (data.Action == "state-edit")
            {
                _Edit(data);
            }
            else if (data.Action == "state-remove")
            {
                _Remove(data);
            }

            return this;
        }

        /// <summary>
        /// Add a new state to the database.
        /// </summary>
        /// <param name="data">State information</param>
        /// <returns>`true` if successful, `false` if in error.</returns>
        private bool _Create(StateRestoreRequest data)
        {
            var validated = _AssertState(data);

            if (validated == false)
            {
                return false;
            }

            var q = _db.Query("insert").Table(_table);

            q.Set(_columnDefault, data.IsDefault);
            q.Set(_columnName, data.Name);
            q.Set(_columnPath, data.Path);
            q.Set(_columnShared, data.IsSharedOut);
            q.Set(_columnState, data.State);
            q.Set(_columnTable, data.Table);

            if (_userId != null)
            {
                q.Set(_columnUser, _userId);
            }

            // Dev defined values
            foreach (var item in _set)
            {
                q.Set(item.Key, item.Value);
            }

            // There can be only one default
            if (data.IsDefault)
            {
                _RemoveDefault(data);
            }

            var res = q.Exec();
            var id = res.InsertId();

            _Read(data, id);

            return true;
        }

        /// <summary>
        /// Update a state on the database.
        /// </summary>
        /// <param name="data">State information</param>
        /// <returns>true on success, false on fail</returns>
        private bool _Edit(StateRestoreRequest data)
        {
            // Must have the table and path, otherwise all states would be returned!
            var validated = _AssertState(data);

            if (validated == false)
            {
                return false;
            }

            if (data.Id == "")
            {
                _result.error = "Incomplete data - no id";
                return false;
            }

            var q = _db.Query("update").Table(_table);

            // Values to set in the update
            q.Set(_columnDefault, data.IsDefault);
            q.Set(_columnName, data.Name);
            q.Set(_columnShared, data.IsSharedOut);
            q.Set(_columnState, data.State);

            // Dev defined values
            foreach (var item in _set)
            {
                q.Set(item.Key, item.Value);
            }

            // Conditions
            q.Where(_columnId, data.Id);
            q.Where(_columnTable, data.Table);
            q.Where(_columnPath, data.Path);

            if (_userId != null)
            {
                q.Where(_columnUser, _userId);
            }

            // There can be only one default
            if (data.IsDefault)
            {
                _RemoveDefault(data);
            }

            System.Console.WriteLine("Pre exec");

            var res = q.Exec();

            System.Console.WriteLine(data.Id);

            // Read the new state back to the client-side
            _Read(data, data.Id);

            return true;
        }

        /// <summary>
        /// Read the states from the db.
        /// </summary>
        /// <param name="data">Submitted data</param>
        /// <param name="id">Limit the read to a specific ID</param>
        /// <returns>true on success, false when in error</returns>
        private bool _Read(StateRestoreRequest data, dynamic id = null)
        {
            // Must have the table and path, otherwise all states would be returned!
            var validated = _AssertStateHost(data);

            if (validated == false)
            {
                return false;
            }

            var q = _db.Query("select").Table(_table).Get(_columnId);

            if (_columnDefault != "")
            {
                q.Get(_columnDefault + " as isDefault");
            }

            if (_columnName != "")
            {
                q.Get(_columnName + " as name");
            }

            if (_columnShared != "")
            {
                q.Get(_columnShared + " as isSharedOut");
            }

            if (_columnState != "")
            {
                q.Get(_columnState + " as state");
            }

            if (_columnUser != "")
            {
                q.Get(_columnUser + " as user");
            }

            q.Where(_columnTable, data.Table);
            q.Where(_columnPath, data.Path);

            if (id != null)
            {
                q.Where(_columnId, id);
            }

            // The user id is optional, but there can't be any separation of
            // user states without it!
            if (_userId != null)
            {
                q.Where(r =>
                {
                    r.Where(_columnUser, _userId);
                    r.OrWhere(_columnShared, 1);
                });
            }

            // Dev set conditions
            foreach (var where in _where)
            {
                if (where.Custom != null)
                {
                    where.Custom(q);
                }
                else
                {
                    q.Where(where.Key, where.Value, where.Operator);
                }
            }

            // Run the assembled query
            var res = q.Exec();
            var output = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;

            // Map to the JSON structure that StateRestore expects
            while ((row = res.Fetch()) != null)
            {
                var inner = new Dictionary<string, object>
                {
                    { "id", row["id"] },
                    { "isDefault", row["isDefault"] },
                    { "isSharedIn", _userId != "" && row["user"].ToString() != _userId ? true : false },
                    { "isSharedOut", row["isSharedOut"] },
                    { "isStatic", false },
                    { "name", row["name"] },
                    { "state", row["state"] },
                };

                output.Add(inner);
            }

            _result.data = output;
            
            return true;
        }

        /// <summary>
        /// Delete state(s)
        /// </summary>
        /// <param name="data">Submitted data</param>
        private bool _Remove(StateRestoreRequest data)
        {
            // Must have the table and path, otherwise all states would be returned!
            var validated = _AssertStateHost(data);

            if (validated == false)
            {
                return false;
            }

            var q = _db.Query("delete").Table(_table);

            q.Where(_columnTable, data.Table);
            q.Where(_columnPath, data.Path);

            if (_userId != null)
            {
                q.Where(_columnUser, _userId);
            }

            q.WhereIn(_columnId, data.Ids);
            q.Exec();

            return true;
        }

        /// <summary>
	    /// If there is an existing default, remove it. The client-side will do
	    /// this as well, so we don't need to worry about there being two
	    /// default states shown, despite only returning a single record.
        /// </summary>
        /// <param name="data">Submitted data</param>
        private void _RemoveDefault(StateRestoreRequest data)
        {
            var q = _db.Query("update").Table(_table);

            // Values to set
            q.Set(_columnDefault, 0);

            // Conditions
            q.Where(_columnDefault, 1);
            q.Where(_columnTable, data.Table);
            q.Where(_columnPath, data.Path);

            if (_userId != null)
            {
                q.Where(_columnUser, _userId);
            }

            q.Exec();
        }
    }
}