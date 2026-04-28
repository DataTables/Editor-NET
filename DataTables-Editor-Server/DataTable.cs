// <summary>
// DataTable class for reading tables
// </summary>
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
#else
using System.Web;
#endif
using DataTables.EditorUtil;

namespace DataTables
{
	/// <summary>
	/// This class let's you define the structure of a database, in order for it
	/// to be read and the data returned to DataTables.
	/// 
	/// Typically you will:
	/// 
	/// * Create the instance
	/// * Define the columns
	/// * Process the request
	/// * Return JSON to the client-side
	/// 
	/// You may also wish to add query conditions, or provide extra pre-column
	/// options for features such as ColumnControl.
	/// </summary>
    public class DataTable
	{
		/// <summary>
		/// Library version
		/// </summary>
        public const string Version = Editor.Version;

		/// <summary>
		/// Editor instance used for the processing of the inbound data.
		/// </summary>
		private Editor _editor;

        /// <summary>
        /// List of columns for this instance
        /// </summary>
		private List<Column> _columns = new List<Column>();

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructors
         */

        /// <summary>
        /// Create a new DataTable instance
        /// </summary>
        /// <param name="db">An instance of the DataTables Database class that we can use for the DB connection. Can also be set with the <code>Db()</code> method.</param>
        /// <param name="table">The table name in the database to read and write information from and to. Can also be set with the <code>Table()</code> method.</param>
        /// <param name="pkey">Primary key column name in the table given. Can also be set with the <code>PKey()</code> method.</param>
        public DataTable(Database db = null, string table = null, string pkey = null)
        {
			_editor = new Editor(db, table, pkey);
			_editor.Write(false);
        }

        /// <summary>
        /// Create a new DataTable instance
        /// </summary>
        /// <param name="db">An instance of the DataTables Database class that we can use for the DB connection. Can also be set with the <code>Db()</code> method.</param>
        /// <param name="table">The table name in the database to read and write information from and to. Can also be set with the <code>Table()</code> method.</param>
        /// <param name="pkey">Primary key column names in the table given. Can also be set with the <code>PKey()</code> method.</param>
        public DataTable(Database db, string table, string[] pkey)
        {
			_editor = new Editor(db, table, pkey);
			_editor.Write(false);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Get the response object that has been created by this instance. This
        /// is only useful after <code>process()</code> has been called.
        /// </summary>
        /// <returns>The response object as populated by this instance</returns>
		public DtResponse Data()
		{
			return _editor.Data();
		}

        /// <summary>
        /// Get the database instance used by this instance
        /// </summary>
        /// <returns>Database connection instance</returns>
        public Database Db()
        {
            return _editor.Db();
        }

        /// <summary>
        /// Set the database connection instance
        /// </summary>
        /// <param name="db">Connection instance to set</param>
        /// <returns>Self for chaining</returns>
        public DataTable Db(Database db)
        {
            _editor.Db(db);
            return this;
        }

        /// <summary>
        /// Get the debug state
        /// </summary>
        /// <returns>true if debugging is enabled</returns>
        public bool Debug()
        {
            return _editor.Debug();
        }

        /// <summary>
        /// Set the debug state. If enabled (`true`) Editor will record information
        /// about the SQL queries it makes and return that information in the JSON
        /// sent to the client-side query once the request has been processed.
        /// </summary>
        /// <param name="debug">Flag to for how to set the debug state</param>
        /// <returns>Self for chaining</returns>
        public DataTable Debug(bool debug)
        {
            _editor.Debug(debug);
            return this;
        }

        /// <summary>
        /// Add debug information to the data sent to the client-side.
        /// </summary>
        /// <param name="debug">Debug information to send</param>
        /// <returns></returns>
        public DataTable Debug(string debug)
        {
            _editor.Debug(debug);
            return this;
        }

        /// <summary>
        /// Add debug information to the data sent to the client-side.
        /// </summary>
        /// <param name="debug">Debug information to send</param>
        /// <returns></returns>
        public DataTable Debug(object debug)
        {
            _editor.Debug(debug);
            return this;
        }

		/// <summary>
		/// Get a column instance that has already been added
		/// </summary>
		/// <param name="name">Column name to get</param>
		/// <returns>Colum instance</returns>
		/// <exception cref="Exception">Unknown column name</exception>
		public Column Column(string name)
		{
            for (var i = 0; i < _columns.Count(); i++)
            {
                if (_columns[i].Name() == name)
                {
                    return _columns[i];
                }
            }

            throw new Exception("Unknown column: " + name);
		}

		/// <summary>
		/// Add a single column
		/// </summary>
		/// <param name="column">Column instance</param>
		/// <returns>Self for chaining</returns>
		public DataTable Column(Column column)
		{
			_columns.Add(column);
            _editor.Field(column.Field());

			return this;
		}

        /// <summary>
        /// Get the DOM prefix.
        /// 
        /// Typically primary keys are numeric and this is not a valid ID value in an
        /// HTML document - is also increases the likelihood of an ID clash if multiple
        /// tables are used on a single page. As such, a prefix is assigned to the 
        /// primary key value for each row, and this is used as the DOM ID, so Editor
        /// can track individual rows.
        /// </summary>
        /// <returns>DOM prefix</returns>
        public string IdPrefix()
        {
			return _editor.IdPrefix();
        }

        /// <summary>
        /// Set the DOM prefix.
        /// 
        /// Typically primary keys are numeric and this is not a valid ID value in an
        /// HTML document - is also increases the likelihood of an ID clash if multiple
        /// tables are used on a single page. As such, a prefix is assigned to the 
        /// primary key value for each row, and this is used as the DOM ID, so Editor
        /// can track individual rows.
        /// </summary>
        /// <param name="prefix">Prefix to set</param>
        /// <returns>Self for chaining</returns>
        public DataTable IdPrefix(string prefix)
        {
			_editor.IdPrefix(prefix);

            return this;
        }

        /// <summary>
        /// Get the left joins that are used by this instance
        /// </summary>
        /// <returns>List of LeftJoin objects</returns>
        public List<LeftJoin> LeftJoin()
        {
			return _editor.LeftJoin();
        }

        /// <summary>
        /// Add a left join condition to the DataTable instance, allowing it to operate
        /// over multiple tables. Multiple <code>leftJoin()</code> calls can be made for a
        /// single DataTable instance to join multiple tables.
        ///
        /// A left join is the most common type of join that is used with DataTable
        /// so this method is provided to make its use very easy to configure. Its
        /// parameters are basically the same as writing an SQL left join statement.
        /// </summary>
        /// <param name="table">Table name to do a join onto</param>
        /// <param name="field1">Field from the parent table to use as the join link</param>
        /// <param name="op">Join condition (`=`, '&lt;`, etc)</param>
        /// <param name="field2">Field from the child table to use as the join link</param>
        /// <returns>Self for chaining</returns>
        public DataTable LeftJoin(string table, string field1, string op = null, string field2 = null)
        {
			_editor.LeftJoin(table, field1, op, field2);

            return this;
        }

        /// <summary>
        /// Add a 1-to-many ("mjoin") join to the Editor instance. The way the
        /// join operates is defined by the MJoin class
        /// </summary>
        /// <param name="join">MJoin link to use</param>
        /// <returns>Self for chaining</returns>
        public DataTable MJoin(MJoin join)
        {
            _editor.MJoin(join);

            return this;
        }

        /// <summary>
        /// Set a model to use.
        ///
        /// In keeping with the MVC style of coding, you can define the fields
        /// and their types that you wish to get from the database in a simple
        /// class. DataTable will automatically add fields from the model.
        ///
        /// Note that fields that are defined in the model can also be defined
        /// as <code>Field</code> instances should you wish to add additional
        /// options to a specific field such as formatters or validation.
        /// </summary>
        /// <typeparam name="T">Model to use</typeparam>
        /// <returns>Self for chaining</returns>
        public DataTable Model<T>()
        {
			_editor.Model<T>();

            return this;
        }

        /// <summary>
        /// Set a model to use.
        /// </summary>
        /// <typeparam name="T">Model to use</typeparam>
        /// <returns>Self for chaining</returns>
        public DataTable Model<T>(string tableName)
        {
			_editor.Model<T>(tableName);

            return this;
        }

        /// <summary>
        /// Get the primary key field that has been configured.
        /// 
        /// The primary key must be known to Editor so it will know which rows are being
        /// edited / deleted upon those actions. The default value is 'id'.
        /// </summary>
        /// <returns>Primary key</returns>
        public string[] Pkey()
        {
			return _editor.Pkey();
        }

        /// <summary>
        /// Set the primary key field to use. Please note that at this time
        /// Editor does not support composite primary keys in a table, only a
        /// single field primary key is supported.
        /// 
        /// The primary key must be known to Editor so it will know which rows are being
        /// edited / deleted upon those actions. The default value is 'id'.
        /// </summary>
        /// <param name="id">Primary key column name</param>
        /// <returns>Self for chaining</returns>
        public DataTable Pkey(string id)
        {
			_editor.Pkey(id);

            return this;
        }

        /// <summary>
        /// Set the column names for a compound primary key.
        /// </summary>
        /// <param name="id">Primary key column names</param>
        /// <returns>Self for chaining</returns>
        public DataTable Pkey(string[] id)
        {
			_editor.Pkey(id);

            return this;
        }
	
        /// <summary>
        /// Process a request from the DataTable client-side to get data.
        /// </summary>
        /// <param name="data">Data sent from the client-side</param>
        /// <returns>Self for chaining</returns>
        public DataTable Process(DtRequest data)
		{
			_editor.Process(data);

			return this;
		}

        /// <summary>
        /// Process a request from the Editor client-side to get / set data.
        /// For use with WebAPI's 'FormDataCollection' collection
        /// </summary>
        /// <param name="data">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Self for chaining</returns>
        public DataTable Process(IEnumerable<KeyValuePair<string, string>> data = null, string culture=null)
        {
			_editor.Process(data, culture);

			return this;
        }

#if NETCOREAPP
        /// <summary>
        /// Get the form action. For use with WebAPI's 'FormDataCollection' collection
        /// </summary>
        /// <param name="data">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Request type</returns>
        public DataTable Process(IEnumerable<KeyValuePair<String, StringValues>> data = null, string culture=null)
        {
			_editor.Process(data, culture);

			return this;
        }
#endif

        /// <summary>
        /// Process a request from the Editor client-side to get / set data.
        /// For use with MVC's 'Request.Form' collection
        /// </summary>
        /// <param name="data">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Self for chaining</returns>
        public DataTable Process(NameValueCollection data = null, string culture=null)
        {
			_editor.Process(data, culture);

			return this;
        }

        /// <summary>
        /// Process a request from the Editor client-side to get / set data.
        /// For use with an HttpRequest object
        /// </summary>
        /// <param name="request">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Self for chaining</returns>
        public DataTable Process(HttpRequest request, string culture=null)
        {
			_editor.Process(request, culture);

			return this;
        }

#if NETFRAMEWORK
        /// <summary>
        /// Process a request from the Editor client-side to get / set data.
        /// For use with an HttpRequest object
        /// </summary>
        /// <param name="request">Data sent from the client-side</param>
        /// <param name="culture">Culture string to use for number formatting - https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo</param>
        /// <returns>Self for chaining</returns>
        public DataTable Process(UnvalidatedRequestValues request, string culture=null)
        {
			_editor.Process(request, culture);

			return this;
        }
#endif

        /// <summary>
        /// Get the database table name this Editor instance will use
        /// </summary>
        /// <returns>Table name</returns>
        public List<string> Table()
        {
			return _editor.Table();
        }

        /// <summary>
        /// Set the database table name this Editor instance will use
        /// </summary>
        /// <param name="t">Table name</param>
        /// <returns>Self for chaining</returns>
        public DataTable Table(string t)
        {
			 _editor.Table(t);

            return this;
        }

        /// <summary>
        /// Add multiple tables to the Editor instance
        /// </summary>
        /// <param name="tables">Collection of tables to add</param>
        /// <returns>Self for chaining</returns>
        public DataTable Table(IEnumerable<string> tables)
        {
			 _editor.Table(tables);

            return this;
        }

        /// <summary>
        /// Where condition to add to the query used to get data from the database.
        /// Multiple conditions can be added if required.
        /// 
        /// Can be used in two different ways:
        /// 
        /// * Simple case: `where( field, value, operator )`
        /// * Complex: `where( fn )`
        ///
        /// The simple case is fairly self explanatory, a condition is applied to the
        /// data that looks like `field operator value` (e.g. `name = 'Allan'`). The
        /// complex case allows full control over the query conditions by providing a
        /// closure function that has access to the database Query that Editor is
        /// using, so you can use the `where()`, `or_where()`, `and_where()` and
        /// `where_group()` methods as you require.
        ///
        /// Please be very careful when using this method! If an edit made by a user
        /// using Editor removes the row from the where condition, the result is
        /// undefined (since Editor expects the row to still be available, but the
        /// condition removes it from the result set).
        /// </summary>
        /// <param name="fn">Delegate to execute adding where conditions to the table</param>
        /// <returns>Self for chaining</returns>
        public DataTable Where(Action<Query> fn)
        {
			_editor.Where(fn);

			return this;
        }

        /// <summary>
        /// Where condition to add to the query used to get data from the database.
        /// Multiple conditions can be added if required.
        /// 
        /// Can be used in two different ways:
        /// 
        /// * Simple case: `where( field, value, operator )`
        /// * Complex: `where( fn )`
        ///
        /// The simple case is fairly self explanatory, a condition is applied to the
        /// data that looks like `field operator value` (e.g. `name = 'Allan'`). The
        /// complex case allows full control over the query conditions by providing a
        /// closure function that has access to the database Query that Editor is
        /// using, so you can use the `where()`, `or_where()`, `and_where()` and
        /// `where_group()` methods as you require.
        ///
        /// Please be very careful when using this method! If an edit made by a user
        /// using Editor removes the row from the where condition, the result is
        /// undefined (since Editor expects the row to still be available, but the
        /// condition removes it from the result set).
        /// </summary>
        /// <param name="key">Database column name to perform the condition on</param>
        /// <param name="value">Value to use for the condition</param>
        /// <param name="op">Conditional operator</param>
        /// <returns>Self for chaining</returns>
        public DataTable Where(string key, object value, string op = "=")
        {
			_editor.Where(key, value, op);

			return this;
        }
	}
}