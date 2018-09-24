// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// DataTables database abstraction library core class.
// </summary>
using System;
using System.Collections.Generic;
using System.Data.Common;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// DataTables Database connection object.
    /// 
    /// Create a database connection which may then have queries performed upon it.
    /// 
    /// This is a database abstraction class that can be used on multiple different
    /// databases. As a result of this, it might not be suitable to perform complex
    /// queries through this interface or vendor specific queries, but everything 
    /// required for basic database interaction is provided through the abstracted
    /// methods.
    /// </summary>
    public class Database : IDisposable
    {
        private string _Adapter;
        private string _DbType;
        private List<DebugInfo> _debugInfo = new List<DebugInfo>();
        private DbConnection _Conn;
        internal DbTransaction DbTransaction = null;
        private bool _DbInited = false;
        private Action<object> _DebugCallback = null;

        /// <summary>
        /// Create a database connection
        /// </summary>
        /// <param name="dbType">Database type - this should be "sqlserver" or "mysql"</param>
        /// <param name="str">Connection string to connect to the SQL server</param>
        /// <param name="adapter">Set the database provider factory</param>
        public Database(string dbType, string str, string adapter = null)
        {
            if (adapter != null)
            {
                Adapter(adapter);
            }

            _DbType = dbType;
            var provider = DbProviderFactories.GetFactory(Adapter());
            _Conn = provider.CreateConnection();
            _Conn.ConnectionString = str;
            _Conn.Open();
        }

        /// <summary>
        /// Create a database connection
        /// </summary>
        /// <param name="dbType">Database type - this should be "sqlserver" or "mysql"</param>
        /// <param name="builder">Connection string builder instance to connect to the SQL server</param>
        public Database(string dbType, DbConnectionStringBuilder builder)
        {
            _DbType = dbType;

            _Conn.ConnectionString = builder.ConnectionString;
            _Conn.Open();
        }

        /// <summary>
        /// Create a database connection
        /// </summary>
        /// <param name="dbType">Database type - this should be "sqlserver" or "mysql"</param>
        /// <param name="conn">Database connection that has already been established to the SQL server</param>
        public Database(string dbType, DbConnection conn)
        {
            _DbType = dbType;
            _Conn = conn;
            _Conn.Open();
        }

        /// <summary>
        /// Get the current debug state for this Database instance
        /// </summary>
        /// <returns>true if debug recording is enabled, false otherwise</returns>
        public bool Debug()
        {
            return _DebugCallback != null;
        }

        /// <summary>
        /// Set the debug state. true will enable recording of SQL statements
        /// </summary>
        /// <param name="debug">Debug flag</param>
        /// <returns>Self for chaining</returns>
        public Database Debug(bool debug)
        {
            if (debug)
            {
                throw new Exception("Debug mode is enabled by adding a callback");
            }
 
            _DebugCallback = null;
            return this;
        }

        public Database Debug(Action<object> callback)
        {
            _DebugCallback = callback;
            return this;
        }

        /// <summary>
        /// Record debug information
        /// </summary>
        /// <param name="sql">Query</param>
        /// <param name="bindings">Bidnings</param>
        /// <returns>Self for chaining</returns>
        internal Database DebugInfo(string sql, List<Binding> bindings)
        {
            if (_DebugCallback != null)
            {
                _DebugCallback(new DebugInfo
                {
                    Query = sql,
                    Bindings = bindings
                });
            }

            return this;
        }

        /// <summary>
        /// Get the database provider factory
        /// </summary>
        /// <returns>Provider factory name</returns>
        public string Adapter()
        {
            if (_Adapter != null)
            {
                return _Adapter;
            }

            switch (_DbType)
            {
                case "mysql":
                    return "MySql.Data.MySqlClient";

                case "oracle":
                    return "Oracle.ManagedDataAccess.Client";

                case "postgres":
                    return "Npgsql";

                case "sqlite":
                    return "Microsoft.Data.Sqlite";

                case "sqlserverce":
                    return "System.Data.SqlServerCe";

                case "azure":
                case "sqlserver":
                    return "System.Data.SqlClient";

                default:
                    throw new Exception("Uknown database type specified");
            }
        }

        /// <summary>
        /// Set the database provider factory
        /// </summary>
        /// <param name="set">Provider factory name</param>
        /// <returns>Self for chaining</returns>
        public Database Adapter(string set)
        {
            _Adapter = set;

            return this;
        }

        /// <summary>
        /// Determine if there is any data in the table that matches the query condition
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="where">Where condition for what to select</param>
        /// <returns>Boolean flag - true if there were rows</returns>
        public bool Any(string table, Action<Query> where = null)
        {
            var res = Query("select")
                .Table(table)
                .Get("*")
                .Where(where)
                .Exec();

            return res.Count() > 0;
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        /// <returns>Self for chaining</returns>
        public Database Commit()
        {
            DataTables.Query.Commit(this);
            return this;
        }

        /// <summary>
        /// Get the database connection
        /// </summary>
        /// <returns>Database connection</returns>
        public DbConnection Conn()
        {
            return _Conn;
        }

        /// <summary>
        /// Perform a delete query on a table.
        /// 
        /// This is a short cut method that creates and update query and then uses the
        /// <code>query('delete')</code>, table, where and exec methods of the query.
        /// </summary>
        /// <param name="table">Table to operate the delete on</param>
        /// <param name="where">Collection of conditions to apply to the delete to</param>
        /// <returns>Result instance</returns>
        public Result Delete(string table, Dictionary<string, dynamic> where)
        {
            return Query("delete")
                    .Table(table)
                    .Where(where)
                    .Exec();
        }

        /// <summary>
        /// Dispose of this database instance
        /// </summary>
        public void Dispose()
        {
            _Conn.Close();
        }

        /// <summary>
        /// Insert data into a table.
        /// 
        /// This is a short cut method that creates an update query and then uses
        /// the <code>query('insert')</code>, table, set and exec methods of the query.
        /// </summary>
        /// <param name="table">Table to perform the insert on</param>
        /// <param name="set">Dictionary of field names and values to set</param>
        /// <param name="pkey">Primary key column names (this is an array for forwards
        /// compt, although only the first item in the array is actually used). This
        /// doesn't need to be set, but it must be if you want to use the
        /// `Result.InsertId()` method.</param>
        /// <returns>Result instance</returns>
        public Result Insert(string table, Dictionary<string, dynamic> set, string[] pkey=null)
        {
            return Query("insert")
                .Pkey(pkey)
                .Table(table)
                .Set(set)
                .Exec();
        }

        /// <summary>
        /// Update or Insert data. When doing an insert, the where condition is
        /// added as a set field
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="set">Dictionary of field names and values to update / set</param>
        /// <param name="where">Where condition for what to update</param>
        /// <param name="pkey">Primary key column names (this is an array for forwards
        /// compt, although only the first item in the array is actually used). This
        /// doesn't need to be set, but it must be if you want to use the
        /// `Result.InsertId()` method.</param>
        /// <returns>Result instance</returns>
        public Result Push(string table, Dictionary<string, dynamic> set, Dictionary<string, dynamic> where, string[] pkey=null)
        {
            var selectColumns = new[] { "*" };

            if (pkey != null)
            {
                selectColumns = pkey;
            }

            // Update or insert
            if (Select(table, selectColumns, where).Count() > 0)
            {
                return Update(table, set, where);
            }

            // Add the where condition to the values to set
            foreach (KeyValuePair<string, dynamic> pair in where)
            {
                if (!set.ContainsKey(pair.Key))
                {
                    set.Add(pair.Key, pair.Value);
                }
            }

            return Insert(table, set, pkey);
        }

        /// <summary>
        /// Create a query object to build a database query.
        /// </summary>
        /// <param name="type">
        /// Database type - this can be 'mysql', 'oracle', 'sqlite' or
        /// 'sqlserver'
        /// </param>
        /// <returns>Query for the database type given</returns>
        public Query Query(string type)
        {
            // Provide a hook that can be used to run any initial commands
            if (!_DbInited)
            {
                switch (_DbType)
                {
                    case "mysql":
                        DatabaseUtil.Mysql.Query.Init(this);
                        break;

                    case "oracle":
                        DatabaseUtil.Oracle.Query.Init(this);
                        break;

                    case "postgres":
                        DatabaseUtil.Postgres.Query.Init(this);
                        break;

                    case "sqlite":
                        DatabaseUtil.Sqlite.Query.Init(this);
                        break;

                    case "azure":
                    case "sqlserver":
                    case "sqlserverce":
                        DatabaseUtil.Sqlserver.Query.Init(this);
                        break;

                    default:
                        throw new Exception("Unknown Database type: " + type);
                }

                _DbInited = true;
            }

            switch (_DbType)
            {
                case "mysql":
                    return new DatabaseUtil.Mysql.Query(this, type);

                case "oracle":
                    return new DatabaseUtil.Oracle.Query(this, type);

                case "postgres":
                    return new DatabaseUtil.Postgres.Query(this, type);

                case "sqlite":
                    return new DatabaseUtil.Sqlite.Query(this, type);

                case "azure":
                case "sqlserver":
                case "sqlserverce":
                    return new DatabaseUtil.Sqlserver.Query(this, type);

                default:
                    throw new Exception("Unknown Database type: " + type);
            }
        }

        /// <summary>
        /// Create a query object to build a database query.
        /// </summary>
        /// <param name="type">
        /// Database type - this can be 'mysql', 'oracle', 'sqlite' or
        /// 'sqlserver'
        /// </param>
        /// <param name="table">
        /// Table to setup this query to execute against
        /// </param>
        /// <returns>Query for the database type given</returns>
        public Query Query(string type, string table)
        {
            return Query(type).Table(table);
        }

        /// <summary>
        /// Rollback the database state to the start of the transaction.
        /// </summary>
        /// <returns>Self for chaining</returns>
        public Database Rollback()
        {
            DataTables.Query.Rollback(this);
            return this;
        }

        /// <summary>
        /// Select data from a table.
        /// 
        /// This is a short cut method that creates an update query and then uses
        /// the <code>query('select')</code>, table, get, where and exec methods
        /// of the query.
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="field">Collection of field names to get. If null all fields are returned</param>
        /// <param name="where">Where condition for what to select</param>
        /// <param name="orderBy">Order by condition</param>
        /// <returns>Result instance</returns>
        public Result Select(string table, IEnumerable<string> field = null, Dictionary<string, dynamic> where = null, IEnumerable<string> orderBy = null)
        {
            if (field == null)
            {
                field = new[] { "*" };
            }

            return Query("select")
                .Table(table)
                .Get(field)
                .Where(where)
                .Order(orderBy)
                .Exec();
        }

        /// <summary>
        /// Select data from a table.
        /// 
        /// This is a short cut method that creates an update query and then uses
        /// the <code>query('select')</code>, table, get, where and exec methods
        /// of the query.
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="field">Collection of field names to get. If null all fields are returned</param>
        /// <param name="where">Where condition for what to select</param>
        /// <param name="orderBy">Order by condition</param>
        /// <returns>Result instance</returns>
        public Result Select(string table, IEnumerable<string> field = null, Action<Query> where = null, IEnumerable<string> orderBy = null)
        {
            if (field == null)
            {
                field = new[] { "*" };
            }

            return Query("select")
                .Table(table)
                .Get(field)
                .Where(where)
                .Order(orderBy)
                .Exec();
        }

        /// <summary>
        /// Select distinct data from a table.
        /// 
        /// This is a short cut method that creates an update query and then uses the
        /// <code>query('select')</code>, distinct ,table, get, where and exec methods of the
        /// query.
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="field">Collection of field names to get. If null all fields are returned</param>
        /// <param name="where">Where condition for what to select</param>
        /// <param name="orderBy">Order by condition</param>
        /// <returns>Result instance</returns>
        public Result SelectDistinct(string table, IEnumerable<string> field = null, Dictionary<string, dynamic> where = null, IEnumerable<string> orderBy = null)
        {
            if (field == null)
            {
                field = new[] { "*" };
            }

            return Query("select")
                .Table(table)
                .Distinct(true)
                .Get(field)
                .Where(where)
                .Order(orderBy)
                .Exec();
        }

        /// <summary>
        /// Select distinct data from a table.
        /// 
        /// This is a short cut method that creates an update query and then uses the
        /// <code>query('select')</code>, distinct ,table, get, where and exec methods of the
        /// query.
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="field">Collection of field names to get. If null all fields are returned</param>
        /// <param name="where">Where condition for what to select</param>
        /// <param name="orderBy">Order by condition</param>
        /// <returns>Result instance</returns>
        public Result SelectDistinct(string table, IEnumerable<string> field = null, Action<Query> where = null, string orderBy = null)
        {
            if (field == null)
            {
                field = new[] { "*" };
            }

            return Query("select")
                .Table(table)
                .Distinct(true)
                .Get(field)
                .Where(where)
                .Order(orderBy)
                .Exec();
        }

        /// <summary>
        /// Select distinct data from a table.
        /// 
        /// This is a short cut method that creates an update query and then uses the
        /// <code>query('select')</code>, distinct ,table, get, where and exec methods of the
        /// query.
        /// </summary>
        /// <param name="table">Table name to act upon</param>
        /// <param name="field">Collection of field names to get. If null all fields are returned</param>
        /// <param name="where">Where condition for what to select</param>
        /// <param name="orderBy">Order by condition</param>
        /// <returns>Result instance</returns>
        public Result SelectDistinct(string table, IEnumerable<string> field = null, Action<Query> where = null, IEnumerable<string> orderBy = null)
        {
            if (field == null)
            {
                field = new[] { "*" };
            }

            return Query("select")
                .Table(table)
                .Distinct(true)
                .Get(field)
                .Where(where)
                .Order(orderBy)
                .Exec();
        }

        /// <summary>
        /// Execute an raw SQL query - i.e. give the method your own SQL, rather
        /// than having the Database classes building it for you.
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <returns>Result instance</returns>
        public Result Sql(string sql)
        {
            return Query("raw").Exec(sql);
        }

        /// <summary>
        /// Start a new database transaction.
        /// </summary>
        /// <returns>Self for chaining</returns>
        public Database Transaction()
        {
            DataTables.Query.Transaction(this);
            return this;
        }

        /// <summary>
        /// Update data.
        ///
        /// This is a short cut method that creates an update query and then uses
        /// the <code>query('update')</code>, table, set, where and exec methods
        /// of the query.
        /// </summary>
        /// <param name="table">Table name to operate on</param>
        /// <param name="set">Field names and values to set</param>
        /// <param name="where">Where condition for what to update</param>
        /// <returns>Self for chaining</returns>
        public Result Update(string table, Dictionary<string, dynamic> set, Dictionary<string, dynamic> where)
        {
            return Query("update")
                .Table(table)
                .Set(set)
                .Where(where)
                .Exec();
        }
    }
}
