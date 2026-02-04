// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Class to define an individual database query
// </summary>
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Text.RegularExpressions;
using DataTables.DatabaseUtil;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// The Query class provides methods to craft an individual query
    /// against the database.
    /// 
    /// The typical pattern for using this class is through the 'Database'.
    /// Typically it would not be initialised directly.
    ///
    /// Note that this is a stub class that a driver will extend and complete as
    /// required for individual database types. Individual drivers could add
    /// additional methods, but this is discouraged to ensure that the API is the
    /// same for all database types. 
    /// </summary>
    abstract public class Query
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructor
         */

        /// <summary>
        /// Query instance construtor. Should be called by the Database
        /// class methods rather than direction initialisation.
        /// </summary>
        /// <param name="db">Database host</param>
        /// <param name="type">Query type</param>
        protected Query(Database db, string type)
        {
            _db = db;
            _type = type;
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Protected properties
         */
        internal string _type;

        internal string _groupBy;

        internal Database _db;

        internal DbCommand _stmt;

        internal List<string> _table = new List<string>();

        internal List<string> _field = new List<string>();

        internal List<Binding> _bindings = new List<Binding>();

        internal List<Where> _where = new List<Where>();

        internal List<string> _join = new List<string>();

        internal List<string> _order = new List<string>();

        internal Dictionary<string, object> _noBind = new Dictionary<string, object>();

        internal int _limit = -1;

        internal int _offset = -1;

        internal bool _distinct = false;

        internal virtual string _bindChar => "@";

        internal virtual string[] _identifierLimiter => null;

        internal virtual string _fieldQuote => "'";

        internal string[] _pkey;

        private int _whereInCnt = 1;

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Static methods
         */

        /// <summary>
        /// Commit a transaction
        /// </summary>
        /// <param name="dbh">The Db instance to use</param>
        public static void Commit(Database dbh)
        {
            dbh.DbTransaction.Commit();
            dbh.DbTransaction.Dispose();
            dbh.DbTransaction = null;
        }

        /// <summary>
        /// Method that can be used by the database driver to run commands on first connect
        /// </summary>
        /// <param name="dbh">Database instance</param>
        public static void Init(Database dbh)
        { }

        /// <summary>
        /// Start a new transaction
        /// </summary>
        /// <param name="dbh">The Db instance to use</param>
        public static void Transaction(Database dbh)
        {
            if (dbh.DbTransaction != null)
            {
                throw new Exception("Already in a transaction. Please close the exisiting transaction first");
            }

            var conn = dbh.Conn();
            dbh.DbTransaction = conn.BeginTransaction();
        }

        /// <summary>
        /// Rollback the database state to the start of the transaction
        /// </summary>
        /// <param name="dbh">The Db instance to use</param>
        public static void Rollback(Database dbh)
        {
            dbh.DbTransaction.Rollback();
            dbh.DbTransaction.Dispose();
            dbh.DbTransaction = null;
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Instance methods
         */

        /// <summary>
        /// Bind a value for safe SQL execution
        /// </summary>
        /// <param name="name">Parameter name - should include the leading escape
        /// character (typically a colon or @)</param>
        /// <param name="value">Value to bind</param>
        /// <param name="type">Data type</param>
        /// <returns>Query instance for chaining</returns>
        public Query Bind(string name, dynamic value, dynamic type = null)
        {
            _bindings.Add(new Binding
            {
                Name = _SafeBind(name),
                Value = value,
                Type = type
            });

            return this;
        }
        
        /// <summary>
        /// Generate a unique name for binding values.
        /// </summary>
        /// <returns></returns>
        public string BindName()
        {
            return _bindChar + "binding_" + _bindings.Count;
        }

        /// <summary>
        /// Set a distinct flag for a `select` query. Note that this has no
        /// effect on any other query type.
        /// </summary>
        /// <param name="dis">Distinct select (`true`) or not (`false`)</param>
        /// <returns>Query instance for chaining</returns>
        public Query Distinct(bool dis)
        {
            _distinct = dis;
            return this;
        }

        /// <summary>
        /// Execute the setup query
        /// </summary>
        /// <param name="sql">SQL string to execute (only if type is 'raw')</param>
        /// <returns>Query result</returns>
        public Result Exec(string sql = null)
        {
            string type = _type.ToLower();

            if (type == "select")
            {
                return _Select();
            }
            else if (type == "insert")
            {
                return _Insert();
            }
            else if (type == "update")
            {
                return _Update();
            }
            else if (type == "delete")
            {
                return _Delete();
            }
            else if (type == "count")
            {
                return _Count();
            }
            else if (type == "raw")
            {
                return _Raw(sql);
            }

            throw new Exception("Unknown database command or not supported: " + sql);
        }

        /// <summary>
        /// Columns to get
        /// </summary>
        /// <returns>List of column names</returns>
        public List<string> Get()
        {
            return _field;
        }

        /// <summary>
        /// A column name to get
        /// </summary>
        /// <param name="field">Column name to get</param>
        /// <returns>Query instance for chaining</returns>
        public Query Get(string field)
        {
            _field.Add(field);

            return this;
        }

        /// <summary>
        /// Add one or more get (select) field
        /// </summary>
        /// <param name="fields">List of column names to get</param>
        /// <returns>Query instance for chaining</returns>
        public Query Get(IEnumerable<string> fields)
        {
            foreach (var field in fields)
            {
                Get(field);
            }

            return this;
        }

        /// <summary>
        /// Add string representing the field to group by
        /// </summary>
        /// <param name="groupBy">The string for the group by</param>
        /// <returns>Query instance for chaining</returns>
        public Query GroupBy(string groupBy)
        {
            this._groupBy = groupBy;

            return this;
        }

        /// <summary>
        /// Determine if the query has any conditions applied to it.
        /// </summary>
        /// <returns>`true` if it has, `false` otherwise</returns>
        public bool HasConditions()
        {
            return _where.Count == 0 ? false : true;
        }

        /// <summary>
        /// Perform a JOIN operation
        /// </summary>
        /// <param name="table">Table name to do the JOIN on</param>
        /// <param name="condition">JOIN condition</param>
        /// <param name="type">JOIN type</param>
        /// <returns>Query instance for chaining</returns>
        public Query Join(string table, string condition, string type = "", bool bind = true)
        {
            string[] joinTypes = new string[] { "LEFT", "RIGHT", "INNER", "OUTER", "LEFT OUTER", "RIGHT OUTER" };

            // Tidy and check we know what the join type is
            if (type != "")
            {
                type = type.ToUpper().Trim();

                if (Array.IndexOf(joinTypes, type) == -1)
                {
                    type = "";
                }
            }

            // Protect the identifiers
            Regex r = new Regex(@"([\w\.]+)([\W\s]+)(.+)");
            Match m = r.Match(condition);

            if (bind && m.Success)
            {
                string cap1 = _ProtectIdentifiers(m.Groups[1].Value);
                string cap3 = _ProtectIdentifiers(m.Groups[3].Value);

                condition = cap1 + m.Groups[2].Value + cap3;
            }

            _join.Add(type + " JOIN " + _ProtectIdentifiers(table) + " ON " + condition + " ");

            return this;
        }

        /// <summary>
        /// Add a collection of left joins to the query
        /// </summary>
        /// <param name="leftJoin">Left join list</param>
        /// <returns>Query instance for chaining</returns>
        public Query LeftJoin(List<LeftJoin> leftJoin)
        {
            foreach (var join in leftJoin)
            {
                if (join.Field2 == null && join.Operator == null ) {
                    this.Join(join.Table, join.Field1, "LEFT", false);
                }
                else {
                    this.Join(join.Table, join.Field1 + " " + join.Operator + " " + join.Field2, "LEFT");
                }
            }

            return this;
        }

        /// <summary>
        /// Limit the result set to a certain size
        /// </summary>
        /// <param name="lim">The number of records to limit the result to</param>
        /// <returns>Query instance for chaining</returns>
        public Query Limit(int lim)
        {
            _limit = lim;
            return this;
        }

        /// <summary>
        /// Offset the return set by a given number of records (useful for paging).
        /// </summary>
        /// <param name="off">The number of records to offset the result by</param>
        /// <returns>Query instance for chaining</returns>
        public Query Offset(int off)
        {
            _offset = off;
            return this;
        }

        /// <summary>
        /// Order by
        /// </summary>
        /// <param name="order">Columns and direction to order by. Can be specified as individual
        /// names or a string of comma separated names. The 'asc' and 'desc' for each column
        /// (as in SQL) is optional.</param>
        /// <returns>Query instance for chaining</returns>
        public Query Order(string order)
        {
            string direction;
            string identifier;
            int idx;

            if (order == null)
            {
                return this;
            }

            string[] ordering = order.Split(new [] {','});

            for (int i = 0; i < ordering.Length; i++)
            {
                // Simplify the white-space
                ordering[i] = ordering[i].Replace('\t', ' ');

                // Find the identifier so we don't escape that
                idx = ordering[i].IndexOf(" ");
                if (idx != -1)
                {
                    direction = ordering[i].Substring(idx);
                    identifier = ordering[i].Substring(0, idx);
                }
                else
                {
                    direction = "";
                    identifier = ordering[i];
                }

                _order.Add(_ProtectIdentifiers(identifier) + " " + direction);
            }

            return this;
        }

        /// <summary>
        /// Order by
        /// </summary>
        /// <param name="orders">List of columns and direction to order by. Can be specified as
        /// individual names or a string of comma separated names. The 'asc' and 'desc' for each
        /// column (as in SQL) is optional.</param>
        /// <returns>Query instance for chaining</returns>
        public Query Order(IEnumerable<string> orders)
        {
            if (orders != null)
            {
                foreach (string order in orders)
                {
                    Order(order);
                }
            }

            return this;
        }

        /// <summary>
        /// Get the primary key column name(s) that have been set for an insert
        /// </summary>
        /// <returns>Primary key names</returns>
        public string[] Pkey()
        {
            return _pkey;
        }

        /// <summary>
        /// Set the primary key column names for an insert, so the inserted value can be
        /// retrieved in the result.
        /// </summary>
        /// <param name="pkey">Primary key column names</param>
        /// <returns>Query instance for chaining</returns>
        public Query Pkey(string[] pkey)
        {
            _pkey = pkey;

            return this;
        }

        /// <summary>
        /// Set a single field to a given value
        /// </summary>
        /// <param name="field">Field name to set</param>
        /// <param name="val">Value to set</param>
        /// <param name="bind">Bind (i.e. escape) the value, or not. Set to false
        /// if you want to use a field reference or function as the value</param>
        /// <returns>Query instance for chaining</returns>
        public Query Set(string field, dynamic val, Boolean bind = true)
        {
            if (field != null)
            {
                _field.Add(field);

                if (bind)
                {
                    Bind(_bindChar + field, val);
                }
                else
                {
                    _noBind.Add(field, val);
                }
            }

            return this;
        }

        /// <summary>
        /// Set a single field to a given value
        /// </summary>
        /// <param name="field">Field name to set</param>
        /// <param name="val">Value to set</param>
        /// <param name="bind">Bind (i.e. escape) the value, or not. Set to false
        /// if you want to use a field reference or function as the value</param>
        /// <param name="type">Db type</param>
        /// <returns>Query instance for chaining</returns>
        public Query Set(string field, dynamic val, Boolean bind, DbType? type)
        {
            if (field != null)
            {
                _field.Add(field);

                if (bind)
                {
                    Bind(_bindChar + field, val, type);
                }
                else
                {
                    _noBind.Add(field, val);
                }
            }

            return this;
        }

        /// <summary>
        /// Set one or more fields to their given values
        /// </summary>
        /// <param name="fields">Key value pairs where the key is the column name</param>
        /// <param name="bind">Bind (i.e. escape) the value, or not. Set to false
        /// if you want to use a field reference or function as the value</param>
        /// <returns>Query instance for chaining</returns>
        public Query Set(Dictionary<string, dynamic> fields, Boolean bind = true)
        {
            if (fields != null)
            {
                foreach (var pair in fields)
                {
                    Set(pair.Key, pair.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Set table(s) to perform the query on
        /// </summary>
        /// <param name="table">Comma separated list of table names</param>
        /// <returns>Query instance for chaining</returns>
        public Query Table(string table)
        {
            if (table == null)
            {
                return null;
            }

            string[] tables = table.Split(new [] {','});

            for (int i = 0; i < tables.Length; i++)
            {
                _table.Add(tables[i].Trim());
            }

            return this;
        }

        /// <summary>
        /// Set table(s) to perform the query on
        /// </summary>
        /// <param name="tables">Collection of table names</param>
        /// <returns>Query instance for chaining</returns>
        public Query Table(List<string> tables)
        {
            if (tables != null)
            {
                for (int i = 0; i < tables.Count; i++)
                {
                    Table(tables[i]);
                }
            }

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="fn">Function that can be used to construct a contained set of options. The Query instance is passed in so Where, AndWhere and OrWhere can all be used</param>
        /// <returns>Query instance for chaining</returns>
        public Query Where(Action<Query> fn)
        {
            if (fn != null)
            {
                _WhereGroup(true, "AND");
                fn(this);
                _WhereGroup(false, "OR");
            }

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="key">Column name to perform the condition on</param>
        /// <param name="value">Value to check. This can be `null` for an `IS NULL` or `IS NOT NULL` condition, depending on the value of `op` which should be `=` or `!=`</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query Where(string key, dynamic value, string op = "=", bool bind = true)
        {
            _Where(key, value, "AND ", op, bind);

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="key">Column name to perform the condition on</param>
        /// <param name="values">Values to check. This can be `null` for an `IS NULL` or `IS NOT NULL` condition, depending on the value of `op` which should be `=` or `!=`</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query Where(string key, IEnumerable<dynamic> values, string op = "=", bool bind = true)
        {
            if (values == null)
            {
                _Where(key, null, "AND ", op, bind);
            }
            else
            {
                foreach (var val in values)
                {
                    _Where(key, val, "AND ", op, bind);
                }
            }

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="set">Dictionary of key (column name) / value pairs to use for the conditions</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query Where(Dictionary<string, dynamic> set, string op = "=", bool bind = true)
        {
            if (set != null)
            {
                foreach (KeyValuePair<string, dynamic> pair in set)
                {
                    Where(pair.Key, pair.Value, op, bind);
                }
            }

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="fn">Function that can be used to construct a contained set of options. The Query instance is passed in so Where, AndWhere and OrWhere can all be used</param>
        /// <returns>Query instance for chaining</returns>
        public Query AndWhere(Action<Query> fn)
        {
            _WhereGroup(true, "AND");
            fn(this);
            _WhereGroup(false, "OR");

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="key">Column name to perform the condition on</param>
        /// <param name="value">Value to check. This can be `null` for an `IS NULL` or `IS NOT NULL` condition, depending on the value of `op` which should be `=` or `!=`</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query AndWhere(string key, dynamic value, string op = "=", bool bind = true)
        {
            return Where(key, value, op, bind);
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="key">Column name to perform the condition on</param>
        /// <param name="values">Values to check. This can be `null` for an `IS NULL` or `IS NOT NULL` condition, depending on the value of `op` which should be `=` or `!=`</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query AndWhere(string key, IEnumerable<dynamic> values, string op = "=", bool bind = true)
        {
            if (values == null)
            {
                _Where(key, null, "AND ", op, bind);
            }
            else
            {
                foreach (var val in values)
                {
                    Where(key, val, op, bind);
                }
            }

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an AND statement
        /// </summary>
        /// <param name="set">Dictionary of key (column name) / value pairs to use for the conditions</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query AndWhere(Dictionary<string, dynamic> set, string op = "=", bool bind = true)
        {
            return Where(set, op, bind);
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an OR statement
        /// </summary>
        /// <param name="fn">Function that can be used to construct a contained set of options. The Query instance is passed in so Where, AndWhere and OrWhere can all be used</param>
        /// <returns>Query instance for chaining</returns>
        public Query OrWhere(Action<Query> fn)
        {
            _WhereGroup(true, "OR");
            fn(this);
            _WhereGroup(false, "OR");

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an OR statement
        /// </summary>
        /// <param name="key">Column name to perform the condition on</param>
        /// <param name="value">Value to check. This can be `null` for an `IS NULL` or `IS NOT NULL` condition, depending on the value of `op` which should be `=` or `!=`</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query OrWhere(string key, dynamic value, string op = "=", bool bind = true)
        {
            _Where(key, value, "OR ", op, bind);

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an OR statement
        /// </summary>
        /// <param name="key">Column name to perform the condition on</param>
        /// <param name="values">Values to check. This can be `null` for an `IS NULL` or `IS NOT NULL` condition, depending on the value of `op` which should be `=` or `!=`</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query OrWhere(string key, IEnumerable<dynamic> values, string op = "=", bool bind = true)
        {
            if (values == null)
            {
                _Where(key, null, "OR ", op, bind);
            }
            else
            {
                foreach (var val in values)
                {
                    _Where(key, val, "OR ", op, bind);
                }
            }

            return this;
        }

        /// <summary>
        /// Where query - Bound to the previous condition (if there is one) as an OR statement
        /// </summary>
        /// <param name="set">Dictionary of key (column name) / value pairs to use for the conditions</param>
        /// <param name="op">Conditional operation to perform</param>
        /// <param name="bind">Bind the value or not. Binding will cause the parameter to effectively be escaped, which you might not want for some cases, such as passing in an SQL function as the condition</param>
        /// <returns>Query instance for chaining</returns>
        public Query OrWhere(Dictionary<string, dynamic> set, string op = "=", bool bind = true)
        {
            foreach (KeyValuePair<string, dynamic> pair in set)
            {
                OrWhere(pair.Key, pair.Value, op, bind);
            }

            return this;
        }

        /// <summary>
        /// Provide grouping for WHERE conditions.
        /// </summary>
        /// <param name="inOut">`true` to open brackets, `false` to close</param>
        /// <param name="op">Conditional operator to use to join to the preceding condition.</param>
        /// <returns>Self for chaining</returns>
        [Obsolete("WhereGroup with a boolean as the first parameter is deprecated, please use WhereGroup with a callback instead.")]
        public Query WhereGroup(bool inOut, string op = "AND")
        {
            _WhereGroup(inOut, op);

            return this;
        }

        /// <summary>
        /// Provide grouping for WHERE conditions.
        /// </summary>
        /// <param name="fn">Callback function which will have any conditions it assigns to the query automatically grouped.</param>
        /// <param name="op">Conditional operator to use to join to the preceding condition.</param>
        /// <returns>Self for chaining</returns>
        public Query WhereGroup(Action<Query> fn, string op = "AND")
        {
            _WhereGroup(true, op);
            fn(this);
            _WhereGroup(false, op);

            return this;
        }

        /// <summary>
        /// Provide a method that can be used to perform a `WHERE ... IN (...)` query with bound values and parameters.
        /// </summary>
        /// <param name="field">Field name to condition on</param>
        /// <param name="values">Values to bind</param>
        /// <param name="op">Conditional operator to use to join to the preceding condition.</param>
        /// <returns></returns>
        public Query WhereIn<T>(string field, ICollection<T> values, string op = "AND")
        {
            if ( values.Count == 0 ) {
                return this;
            }

            var binders = new List<string>();
            var prefix = _bindChar+"wherein";

            foreach (var val in values)
            {
                var binder = prefix + _whereInCnt.ToString();

                Bind(binder, val);

                binders.Add(binder);
                _whereInCnt++;
            }

            _where.Add(new Where()
                .Operator(op)
                .Field(_ProtectIdentifiers(field))
                .Query(_ProtectIdentifiers(field) + " IN (" + String.Join(",", binders)+ ")")
            );

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Protected methods
         */

            /// <summary>
            /// Create a comma separated field list
            /// </summary>
            /// <param name="addAlias">Indicate if the fields should have an `as` alias added automatically (true) or not</param>
            /// <returns>SQL list of fields</returns>
        virtual protected string _BuildField(Boolean addAlias = false)
        {
            List<string> a = new List<string>();
            string field;

            for (int i = 0; i < _field.Count; i++)
            {
                field = _field[i];

                if (addAlias && field != "*")
                {
                    if (field.IndexOf(" as ") != -1)
                    {
                        var split = field.Split(new[] { " as " }, StringSplitOptions.None);
                        a.Add(
                            _ProtectIdentifiers(split[0]) + " as " +
                            _fieldQuote + split[1] + _fieldQuote
                        );
                    }
                    else
                    {
                        var fieldName = field;

                        // If the field has the quoting character in it, it needs to be doubled up to escape
                        if (field.Contains(_fieldQuote))
                        {
                            fieldName = field.Replace(_fieldQuote, _fieldQuote + _fieldQuote);
                        }

                        a.Add(
                            _ProtectIdentifiers(field) + " as " +
                            _fieldQuote + fieldName + _fieldQuote
                        );
                    }
                }
                else
                {
                    a.Add(_ProtectIdentifiers(field));
                }
            }

            return " " + string.Join(", ", a.ToArray()) + " ";
        }

        /// <summary>
        /// Create a GROUP BY satement
        /// </summary>
        /// <returns>SQL GROUP BY statement</returns>
        virtual protected string _BuildGroupBy()
        {
            string output = "";
            if(this._groupBy != null){
                output = " GROUP BY " +_ProtectIdentifiers(this._groupBy);
            }
            return output;
        }

        /// <summary>
        /// Create a JOIN satement list
        /// </summary>
        /// <returns>SQL list of joins</returns>
        virtual protected string _BuildJoin()
        {
            return string.Join(" ", _join.ToArray());
        }

        /// <summary>
        /// Create the LIMIT / OFFSET string.
        /// 
        /// Default is to create a MySQL and Postgres style statement. Drivers can override
        /// </summary>
        /// <returns>SQL limit and offset statement</returns>
        virtual protected string _BuildLimit()
        {
            string limit = "";

            if (_limit >= 0)
            {
                limit = " LIMIT " + _limit.ToString();
            }

            if (_offset >= 0)
            {
                limit = limit + " OFFSET " + _offset.ToString();
            }

            return limit;
        }

        /// <summary>
        /// Create the ORDER BY statement
        /// </summary>
        /// <returns>SQL order statement</returns>
        virtual protected string _BuildOrder()
        {
            if (_order.Count > 0)
            {
                return " ORDER BY " + String.Join(", ", _order.ToArray()) + " ";
            }
            return "";
        }

        /// <summary>
        /// Create a set list
        /// </summary>
        /// <returns>SQL for update</returns>
        virtual protected string _BuildSet()
        {
            var vals = new List<string>();

            for (int i = 0, ien = _field.Count; i < ien; i++)
            {
                var field = _field[i];

                if (_noBind.ContainsKey(field))
                {
                    vals.Add(_ProtectIdentifiers(field) + " = " + _noBind[field]);
                }
                else
                {
                    vals.Add(_ProtectIdentifiers(field) + " = " + _bindChar + _SafeBind(field));
                }
            }

            return " " + String.Join(", ", vals.ToArray()) + " ";
        }

        /// <summary>
        /// Create the table list
        /// </summary>
        /// <returns>SQL table statement</returns>
        virtual protected string _BuildTable()
        {
            var tables = new List<string>();

            for (int i = 0, ien = _table.Count; i < ien; i++)
            {
                var table = _table[i];
                string name = table;

                if (_type == "insert")
                {
                    if (table.IndexOf(" as ") != -1)
                    {
                        var split = table.Split(new[] {" as "}, StringSplitOptions.None);
                        name = split[0];
                    }
                    else if (table.IndexOf(" ") != -1)
                    {
                        var split = table.Split(new[] {" "}, StringSplitOptions.None);
                        name = split[0];
                    }
                }

                tables.Add( _ProtectIdentifiers(name));
            }

            return " " + string.Join(",", tables.ToArray()) + " ";
        }

        /// <summary>
        /// Create a bind field balue list
        /// </summary>
        /// <returns>SQL value list for inserts</returns>
        virtual protected string _BuildValue()
        {
            List<string> vals = new List<string>();

            for (int i = 0, ien = _field.Count; i < ien; i++)
            {
                vals.Add(" " + _bindChar + _SafeBind(_field[i]));
            }

            return " " + String.Join(", ", vals.ToArray()) + " ";
        }

        /// <summary>
        /// Create the WHERE statement
        /// </summary>
        /// <returns>SQL WHERE statement</returns>
        virtual protected string _BuildWhere()
        {
            if (_where.Count == 0)
            {
                return "";
            }

            string condition = "WHERE ";

            for (int i = 0, ien = _where.Count; i < ien; i++)
            {
                if (i == 0)
                {
                    // Nothing (simplifies the logic!)
                }
                else if (_where[i].Group() == ")")
                {
                    // If a group has been used but no conditions were added inside
                    // of, we don't want to end up with `()` in the SQL as that is
                    // invalid, so add a 1.
                    if (_where[i - 1].Group() == "(")
                    {
                        condition += "1=1";
                    }
                    // else nothing
                }
                else if (_where[i - 1].Group() == "(")
                {
                    // Nothing
                }
                else
                {
                    condition += _where[i].Operator();
                }

                if (_where[i].Group() != null)
                {
                    condition += _where[i].Group();
                }
                else
                {
                    condition += _where[i].Query() + " ";
                }
            }

            return condition;
        }

        /// <summary>
        /// Run a SELECT with a COUNT - returns in a `cnt` parameter for the selected row.
        /// </summary>
        /// <returns>Query result</returns>
        virtual protected Result _Count()
        {
            var select = "SELECT COUNT(" + _BuildField() + ") " + _ProtectIdentifiers("cnt");

            _Prepare(
                select
                + " FROM " + _BuildTable()
                + _BuildJoin()
                + _BuildWhere()
                + _BuildGroupBy()
                + _BuildOrder()
                + _BuildLimit()
            );
            
            return _Exec();
        }

        /// <summary>
        /// Execute a DELETE statement from the current configuration
        /// </summary>
        /// <returns>Query result</returns>
        virtual protected Result _Delete()
        {
            _Prepare(
                "DELETE FROM "
                + _BuildTable()
                + _BuildWhere()
            );

            return _Exec();
        }

        /// <summary>
        /// Execute the query. Provided by the driver
        /// </summary>
        /// <returns>Query result</returns>
        virtual protected Result _Exec()
        {
            throw new Exception("_Exec method not overridden by driver");
        }

        /// <summary>
        /// Execute an INSERT statement from the current configuration
        /// </summary>
        /// <returns>Query result</returns>
        virtual protected Result _Insert()
        {
            _Prepare(
                "INSERT INTO " +
                    _BuildTable() + " ("
                        + _BuildField()
                    + ") "
                + "VALUES ("
                    + _BuildValue()
                + ")"
            );

            return _Exec();
        }

        /// <summary>
        /// Prepare the SQL query by populating the bound variables. Provided by the driver
        /// </summary>
        /// <param name="sql">SQL to run</param>
        virtual protected void _Prepare(string sql)
        {
            throw new Exception("_Prepare method not overridden by driver");
        }

        /// <summary>
        /// Protect field names
        /// </summary>
        /// <param name="identifier">Field name</param>
        /// <returns>Quoted field name</returns>
        protected virtual string _ProtectIdentifiers(string identifier)
        {
            var idl = _identifierLimiter;
            string alias;

            // No escaping character
            if (idl == null)
            {
                return identifier;
            }

            var left = idl[0];
            var right = idl[1];

            // Dealing with a function or other expression? Just return immediately
            if (identifier.Contains("(") || identifier.Contains("*"))
            {
                return identifier;
            }

            // Going to be operating on the spaces in the string to
            // simplify the white space
            identifier = identifier.Replace('\t', ' ');
            identifier = identifier.Replace(" as ", " ");

            // If more than a single space, then return
            if (identifier.Split(new [] {' '}).Length > 2) {
                return identifier;
            }

            // Find if our identifier has an alias, so we don't escape that
            var aliasIdx = identifier.IndexOf(" ");

            if (aliasIdx != -1)
            {
                alias = identifier.Substring(aliasIdx);
                identifier = identifier.Substring(0, aliasIdx);
            }
            else
            {
                alias = "";
            }

            var a = identifier.Split(new [] {'.'});
            return left + string.Join(right + '.' + left, a) + right + alias;
        }

        /// <summary>
        /// Execute a given statement
        /// </summary>
        /// <param name="sql">SQL to execute</param>
        /// <returns>Query result</returns>
        virtual protected Result _Raw(string sql)
        {
            _Prepare(sql);

            return _Exec();
        }

        /// <summary>
        /// The characters that can be used to bind a value are quite limited. We need
        /// to abstract this out to allow slightly more complex expressions including
        /// dots for easy aliasing
        /// </summary>
        /// <param name="name">Field name</param>
        /// <returns>Modify field name</returns>
        virtual protected string _SafeBind(string name)
        {
            return name
                .Replace(".", "_1_")
                .Replace("-", "_2_")
                .Replace("/", "_3_")
                .Replace("\\", "_4_");
        }

        /// <summary>
        /// Execute an SELECT statement from the current configuration
        /// </summary>
        /// <returns>Query result</returns>
        virtual protected Result _Select()
        {
            _Prepare(
                "SELECT " + (_distinct ? "DISTINCT " : "")
                + _BuildField(true)
                + "FROM " + _BuildTable()
                + _BuildJoin()
                + _BuildWhere()
                + _BuildGroupBy()
                + _BuildOrder()
                + _BuildLimit()
            );

            return _Exec();
        }

        /// <summary>
        /// Execute a UPDATE statement from the current configuration
        /// </summary>
        /// <returns>Query result</returns>
        virtual protected Result _Update()
        {
            _Prepare(
                "UPDATE "
                + _BuildTable()
                + "SET " + _BuildSet()
                + _BuildWhere()
            );

            return _Exec();
        }

        /// <summary>
        /// Add an individual where condition to the query
        /// </summary>
        /// <param name="key">Wkere key</param>
        /// <param name="value">Value to use</param>
        /// <param name="type">Combination operator</param>
        /// <param name="op">Conditional operator</param>
        /// <param name="bind">Bind flag</param>
        virtual protected void _Where(string key, dynamic value, string type = "AND ", string op = "=", bool bind = true)
        {
            int whereCount = _where.Count;

            if (value == null)
            {
                _where.Add(new Where()
                    .Operator(type)
                    .Field(_ProtectIdentifiers(key))
                    .Query(_ProtectIdentifiers(key) + (op == "=" ?
                        " IS NULL" :
                        " IS NOT NULL"
                    ))
                );
            }
            else if (bind)
            {
                if (this._db.DbType() == "postgres" && op == "like") {
                    _where.Add(new Where()
                        .Operator(type)
                        .Field(_ProtectIdentifiers(key))
                        .Query(_ProtectIdentifiers(key) + "::text ilike " + _bindChar + "where_" + whereCount)
                    );
                }
                else {
                    _where.Add(new Where()
                        .Operator(type)
                        .Field(_ProtectIdentifiers(key))
                        .Query(_ProtectIdentifiers(key) + " " + op + " " + _bindChar + "where_" + whereCount)
                    );
                }
                Bind(_bindChar + "where_" + whereCount, value);
            }
            else
            {
                _where.Add(new Where()
                    .Operator(type)
                    .Query(_ProtectIdentifiers(key) + " " + op + " " + value)
                );
            }
        }

        /// <summary>
        /// Add parentheses to a where condition
        /// </summary>
        /// <param name="inOut">Opening (`true`) or closing bracket</param>
        /// <param name="op">Operator</param>
        virtual protected void _WhereGroup(bool inOut, string op)
        {
            _where.Add(new Where()
                .Group(inOut ? "(" : ")")
                .Operator(op)
            );
        }
    }
}
