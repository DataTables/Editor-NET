﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// The MJoin class provides a one-to-many join link for Editor. This can
    /// be useful in cases were an attribute can take multiple values at the
    /// same time - for example cumulative security access levels.
    /// 
    /// Typically the MJoin class should be used with a link table, but this is
    /// optional. Please note that if you don't use a link table you should be
    /// aware that on edit the linked rows are deleted and then reinserted, thus
    /// if any values should be retained they should also be submitted.
    /// 
    /// Please refer to the Editor .NET documentation for further information
    /// https://editor.datatables.net/manual/net
    /// </summary>
    public class MJoin
    {
        /// <summary>
        /// Create an MJoin instance for use with the Editor class's MJoin
        /// method.
        /// </summary>
        /// <param name="table">Table to join to.</param>
        public MJoin(string table)
        {
            Table(table);
            Name(table.Split(new[] { '.' }).Last());
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private parameters
         */

        private string _table;
        private Editor _editor;
        private string _name;
        private Boolean _get = true;
        private Boolean _set = true;
        private readonly List<WhereCondition> _where = new List<WhereCondition>();
        private readonly List<Field> _fields = new List<Field>();
        private Type _userModelT;
        private readonly List<LeftJoin> _leftJoin = new List<LeftJoin>();
        private readonly List<string> _links = new List<string>();
        private string _linkTable;
        private string _hostField;
        private string _childField;
        private string _linkHostField;
        private string _linkChildField;
        private string _order;
        private List<Func<Editor, DtRequest.RequestTypes, Dictionary<string, object>, string>> _validators = new List<Func<Editor, DtRequest.RequestTypes, Dictionary<string, object>, string>>();
        private List<string> _validatorFields = new List<string>();




        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Add a new field to the MJoin instance
        /// </summary>
        /// <param name="field">New field to add</param>
        /// <returns>Self for chaining</returns>
        public MJoin Field(Field field)
        {
            _fields.Add(field);
            return this;
        }

        /// <summary>
        /// Get the list of fields configured for this join instance
        /// </summary>
        /// <returns>Join instance fields</returns>
        public List<Field> Fields()
        {
            return _fields;
        }

        /// <summary>
        /// Get the get flag for this MJoin instance. If disabled data will not
        /// be retrieved
        /// </summary>
        /// <returns>Enablement status</returns>
        public Boolean Get()
        {
            return _get;
        }

        /// <summary>
        /// Set the get flag for this MJoin instance. If disabled data will not
        /// be retrieved when loaded by DataTables.
        /// </summary>
        /// <param name="flag">Value to set</param>
        /// <returns>Self for chaining</returns>
        public MJoin Get(bool flag)
        {
            _get = flag;
            return this;
        }

        /// <summary>
        /// Add a left join condition to the Mjoin instance, allowing it to operate
        /// over multiple tables. Multiple <code>leftJoin()</code> calls can be made for a
        /// single Mjoin instance to join multiple tables.
        /// </summary>
        /// <param name="table">Table name to do a join onto</param>
        /// <param name="field1">Field from the parent table to use as the join link</param>
        /// <param name="op">Join condition (`=`, '&lt;`, etc)</param>
        /// <param name="field2">Field from the child table to use as the join link</param>
        /// <returns>Self for chaining</returns>
        public MJoin LeftJoin(string table, string field1, string op = null, string field2 = null)
        {
            _leftJoin.Add(new LeftJoin(table, field1, op, field2));

            return this;
        }

        /// <summary>
        /// Create a join link between two tables. The order of the fields does not
        /// matter, but each field must contain the table name as well as the field name.
        /// 
        /// This method can be called a maximum of two times for an MJoin instance:
        /// 
        /// * First time, creates a link between the Editor host table and a join table
        /// * Second time creates the links required for a link table.
        /// 
        /// Please refer to the Editor MJoin documentation for further details:
        /// https://editor.datatables.net/manual/net
        /// </summary>
        /// <param name="field1">Table and field name</param>
        /// <param name="field2">Table and field name</param>
        /// <returns>Self for chaining</returns>
        public MJoin Link(string field1, string field2)
        {
            if (field1.IndexOf('.') == -1 || field2.IndexOf('.') == -1)
            {
                throw new Exception("MJoin fields must contain both the table name and the column name");
            }

            if (_links.Count() == 4)
            {
                throw new Exception("MJoin Link method cannot be called more than twice for a single instance");
            }

            // Add to list - it is resolved later on
            _links.Add(field1);
            _links.Add(field2);

            return this;
        }

        /// <summary>
        /// Set a model to use.
        ///
        /// In keeping with the MVC style of coding, you can define the fields
        /// and their types that you wish to get from the database in a simple
        /// class. Editor will automatically add fields from the model.
        ///
        /// Note that fields that are defined in the model can also be defined
        /// as <code>Field</code> instances should you wish to add additional
        /// options to a specific field such as formatters or validation.
        /// </summary>
        /// <typeparam name="T">Model to use</typeparam>
        /// <returns>Self for chaining</returns>
        public MJoin Model<T>()
        {
            _userModelT = typeof(T);

            return this;
        }

        /// <summary>
        /// Get the JSON name for the join JSON and HTTP submit data.
        /// </summary>
        /// <returns>JSON name</returns>
        public string Name()
        {
            return _name;
        }

        /// <summary>
        /// Set the JSON name for the join JSON and HTTP submit data. By default this
        /// is set to match the table name, but can be altered using this method.
        /// </summary>
        /// <param name="name">Name to use</param>
        /// <returns>Self for chaining</returns>
        public MJoin Name(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Get the current order string
        /// </summary>
        /// <returns>SQL order string</returns>
        public string Order()
        {
            return _order;
        }

        /// <summary>
        /// Set the order to apply to the joined data
        /// </summary>
        /// <param name="order">SQL order string</param>
        /// <returns>Self for chaining</returns>
        public MJoin Order(string order)
        {
            _order = order;
            return this;
        }

        /// <summary>
        /// Get the set value for this instance. If disabled this MJoin instance
        /// will not write to the database
        /// </summary>
        /// <returns>Enablement value</returns>
        public Boolean Set()
        {
            return _set;
        }

        /// <summary>
        /// Set the set value for this instance. If disabled this MJoin instance
        /// will not write to the database on create, edit or delete.
        /// </summary>
        /// <param name="flag">Value to set</param>
        /// <returns>Self for chaining</returns>
        public MJoin Set(bool flag)
        {
            _set = flag;
            return this;
        }

        /// <summary>
        /// Get the DB table name that this MJoin instance links the main table to
        /// </summary>
        /// <returns>Table name</returns>
        public string Table()
        {
            return _table;
        }

        /// <summary>
        /// Set the table name that this MJoin instance links the main table to.
        /// </summary>
        /// <param name="table">Table to link to</param>
        /// <returns>Self for chaining</returns>
        public MJoin Table(string table)
        {
            _table = table;
            return this;
        }

        public MJoin Validator(string fieldName, Func<Editor, DtRequest.RequestTypes, Dictionary<string, object>, string> fn)
        {
            _validators.Add(fn);
            _validatorFields.Add(fieldName);
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
        public MJoin Where(Action<Query> fn)
        {
            _where.Add(new WhereCondition
            {
                Custom = fn
            });

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
        public MJoin Where(string key, dynamic value, string op = "=")
        {
            _where.Add(new WhereCondition
            {
                Key = key,
                Value = value,
                Operator = op
            });

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */

        /// <summary>
        /// Data "get" request - get the joined data
        /// </summary>
        /// <param name="editor">Host Editor instance</param>
        /// <param name="response">DataTables response object for where the data
        /// should be written to</param>
        internal void Data(Editor editor, DtResponse response)
        {
            _Prepare(editor);

            if (response.data.Count() != 0)
            {
                // This is something that will likely come in a future version, but it
                // is a relatively low use feature. Please get in touch if this is
                // something you require.
                var pkeyA = editor.Pkey();
                if (pkeyA.Length > 1)
                {
                    throw new Exception("MJoin is not currently supported with a compound primary key for the main table.");
                }
                
                var readField = "";
                var joinFieldSplit = _hostField.Split(new[] { '.' });
                var joinFieldTable = joinFieldSplit.First();
                var joinFieldName = joinFieldSplit.Last();

                // If the Editor primary key is join key, then it is read automatically
                // and into Editor's primary key store
                var pkeyIsJoin = _hostField == pkeyA[0] || _hostField == editor.Table()[0];

                // Build the basic query
                var query = editor.Db()
                    .Query("select")
                    .Distinct(true)
                    .Get(_hostField + " as dteditor_pkey")
                    .Table(joinFieldTable);

                if (Order() != null)
                {
                    query.Order(Order());
                }

                _ApplyWhere(query);
                query.LeftJoin(_leftJoin);

                foreach (var field in _fields.Where(field => field.Apply("get") && field.GetValue() == null))
                {
                    if (field.DbField().IndexOf('.') == -1)
                    {
                        query.Get(_table + "." + field.DbField() + " as " + field.DbField());
                    }
                    else
                    {
                        query.Get(field.DbField());
                    }
                }

                // Create the joins
                if (_linkTable != null)
                {
                    query.Join(_linkTable, _hostField + " = " + _linkHostField);
                    query.Join(_table, _childField + " = " + _linkChildField);
                }
                else
                {
                    query.Join(_table, _childField + " = " + _hostField);
                }

                if (NestedData.InData(_hostField, response.data[0]))
                {
                    readField = _hostField;
                }
                else if (NestedData.InData(joinFieldName, response.data[0]))
                {
                    readField = joinFieldName;
                }
                else if (!pkeyIsJoin)
                {
                    throw new Exception(
                        "Join was performed on the field '" + _hostField + "' which was not " +
                        "included in the Editor field list. The join field must be " +
                        "included as a regular field in the Editor instance."
                    );
                }

                // There appears to be a bug in the MySQL drivers for .NETCore whereby if there is
                // only one result in the original data set, then it can only read one result into
                // the new data set when using WHERE IN. Any other number of rows is fine. Tried
                // different versions of the server and driver but to no avail, so this is a workaround.
                if (editor.Db().DbType() != "mysql" || response.data.Count != 1)
                {
                    // Get list of pkey values and apply as a WHERE IN condition
                    // This is primarily useful in server-side processing mode and when filtering
                    // the table as it means only a sub-set will be selected
                    // This is only applied for "sensible" data sets. It will just complicate
                    // matters for really large data sets:
                    // https://stackoverflow.com/questions/21178390/in-clause-limitation-in-sql-server
                    if (response.data.Count < 1000)
                    {
                        var whereIn = new List<object>();

                        foreach (var data in response.data)
                        {
                            whereIn.Add(pkeyIsJoin
                                ? (data["DT_RowId"].ToString()).Replace(editor.IdPrefix(), "")
                                : NestedData.ReadProp(readField, data).ToString()
                            );
                        }

                        query.WhereIn(_hostField, whereIn);
                    }
                }

                var result = query.Exec();

                // Map the data to the primary key for fast look up
                var join = new Dictionary<string, List<Dictionary<string, object>>>();
                Dictionary<string, object> row;

                while ((row = result.Fetch()) != null)
                {
                    var inner = new Dictionary<string, object>();

                    foreach (var field in _fields.Where(field => field.Apply("get")))
                    {
                        field.Write(inner, row);
                    }

                    var lookup = row["dteditor_pkey"].ToString();
                    if (!join.ContainsKey(lookup))
                    {
                        join.Add(lookup, new List<Dictionary<string, object>>());
                    }

                    join[lookup].Add(inner);
                }

                // Loop over the data and do a join based on the data available
                foreach (var data in response.data)
                {
                    var linkField = pkeyIsJoin
                        ? (data["DT_RowId"].ToString()).Replace(editor.IdPrefix(), "")
                        : NestedData.ReadProp(readField, data).ToString();

                    data.Add(_name, join.ContainsKey(linkField)
                        ? join[linkField]
                        : new List<Dictionary<string, object>>()
                    );
                }
            }
        }

        /// <summary>
        /// Create a new row
        /// </summary>
        /// <param name="editor">Host Editor instance</param>
        /// <param name="parentId">Parent row id</param>
        /// <param name="data">HTTP submitted data</param>
        internal void Insert(Editor editor, dynamic parentId, Dictionary<string, object> data)
        {
            if (!_set || !data.ContainsKey(_name) || !data.ContainsKey(_name + "-many-count"))
            {
                return;
            }

            _Prepare(editor);
            var list = (Dictionary<string, object>)data[_name];

            foreach (var dataSet in list.Select(item => item.Value as Dictionary<string, object>))
            {
                if (_linkTable != null)
                {
                    // Insert the keys into the join table - note that we need to
                    // remove the table name from the field as some dbs (postgres)
                    // don't like it. Might be nicer to have the Database classes
                    // do this - todo
                    var a = _childField.Split(new[] { '.' });

                    _editor.Db()
                        .Query("Insert")
                        .Table(_linkTable)
                        .Set(_linkHostField.Split(new[] { '.' }).Last(), parentId)
                        .Set(_linkChildField.Split(new[] { '.' }).Last(), dataSet[a.Last()])
                        .Exec();
                }
                else
                {
                    var query = _editor.Db()
                        .Query("Insert")
                        .Table(_table)
                        .Set(_childField.Split(new[] { '.' }).Last(), parentId);

                    foreach (var field in _fields)
                    {
                        if (field.Apply("set", dataSet))
                        {
                            query.Set(field.DbField().Split(new[] { '.' }).Last(), field.Val("set", dataSet));
                        }
                    }

                    query.Exec();
                }
            }
        }

        /// <summary>
        /// Get the options for the fields in this join
        /// </summary>
        /// <param name="options">Options object</param>
        /// <param name="db">Database connection object</param>
        /// <param name="refresh">Refresh indication flag</param>
        internal void Options(Dictionary<string, object> options, Database db, bool refresh)
        {
            // Field options
            foreach (var field in _fields)
            {
                var optsInst = field.Options();

                if (optsInst != null)
                {
                    var opts = optsInst.Exec(db, refresh);
                    
                    if (opts != null)
                    {
                        options.Add(_name + "[]." + field.Name(), opts);
                    }
                }
            }
        }

        /// <summary>
        /// Edit a row
        /// </summary>
        /// <param name="editor">Host Editor instance</param>
        /// <param name="parentId">Parent row id</param>
        /// <param name="data">HTTP submitted data</param>
        internal void Update(Editor editor, dynamic parentId, Dictionary<string, object> data)
        {
            if (!_set || !data.ContainsKey(_name + "-many-count"))
            {
                return;
            }

            // WARNING - this removes rows and then readds them. Any data not in
            // the field list WILL BE LOST.
            Remove(editor, parentId);
            Insert(editor, parentId, data);
        }

        /// <summary>
        /// Remove a row
        /// </summary>
        /// <param name="editor">Host Editor instance</param>
        /// <param name="ids">Parent row id</param>
        internal void Remove(Editor editor, dynamic ids)
        {
            if (!_set)
            {
                return;
            }

            _Prepare(editor);

            if (_linkTable != null)
            {
                var query = editor.Db()
                    .Query("Delete")
                    .Table(_linkTable)
                    .OrWhere(_linkHostField, ids);

                query
                    .Exec();
            }
            else
            {
                var query = editor.Db()
                    .Query("Delete")
                    .Table(_table)
                    .WhereGroup(q =>
                    {
                        q.OrWhere(_childField, ids);
                    });

                _ApplyWhere(query);

                query.Exec();
            }
        }


        /// <summary>
        /// Validate the MJoin fields submitted
        /// </summary>
        /// <param name="response">DataTables response object to record the errors</param>
        /// <param name="editor">Host Editor instance</param>
        /// <param name="data">Data submitted by the client</param>
        internal void Validate(DtResponse response, Editor editor, Dictionary<string, object> data, DtRequest.RequestTypes action)
        {
            if (!_set)
            {
                return;
            }

            _Prepare(editor);
            var list = data.ContainsKey(_name) ?
                (Dictionary<string, object>)data[_name] :
                new Dictionary<string, object>();

            // Grouped validation
            for (var i = 0; i < _validators.Count(); i++)
            {
                var res = _validators[i](editor, action, list);

                if (res != "" && res != null)
                {
                    response.fieldErrors.Add(new DtResponse.FieldError
                    {
                        name = _validatorFields[i],
                        status = res
                    });
                }
            }

            // Field validation
            foreach (var dataSet in list.Select(item => item.Value as Dictionary<string, object>))
            {
                foreach (var field in _fields)
                {
                    var validation = field.Validate(dataSet, editor);

                    if (validation != null)
                    {
                        response.fieldErrors.Add(new DtResponse.FieldError
                        {
                            name = _name + "[]." + field.Name(),
                            status = validation
                        });
                    }
                }
            }
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private methods
         */

        /// <summary>
        /// Apply the where conditions to a query
        /// </summary>
        /// <param name="query">Query to apply the conditions to</param>
        private void _ApplyWhere(Query query)
        {
            foreach (var where in _where)
            {
                if (where.Custom != null)
                {
                    where.Custom(query);
                }
                else
                {
                    query.Where(where.Key, where.Value, where.Operator);
                }
            }
        }

        /// <summary>
        /// Complete initialisation once we have an Editor instance to work with
        /// </summary>
        /// <param name="editor">Host Editor instance</param>
        private void _Prepare(Editor editor)
        {
            _editor = editor;

            // Add the fields from the model
            if (_userModelT != null)
            {
                _PrepareModel();
            }

            var hostTables = _ParentTables();

            // Resolve what field names belong to what varible for processing
            for (int i = 0, ien = _links.Count(); i < ien; i++)
            {
                var a = _links[i].Split(new[] { '.' });

                string tableName = string.Join(".", a.Take(a.Length - 1));

                if (tableName == _table)
                {
                    _childField = _links[i];
                }
                else if (hostTables.Contains(tableName))
                {
                    _hostField = _links[i];
                }
                else
                {
                    _linkTable = tableName;

                    // Need to figure out if the link refers to the host
                    // table, or the child table - this is based on the
                    // partner property that was given with this linking
                    // field when 'Link()' was called.
                    var partner = i == 0 ? _links[1] :
                        i == 1 ? _links[0] :
                        i == 2 ? _links[3] :
                                 _links[2];

                    if (partner.Contains(_table + ".") && partner.IndexOf(_table + ".") == 0)
                    {
                        _linkChildField = _links[i];
                    }
                    else
                    {
                        _linkHostField = _links[i];
                    }
                }
            }
        }

        /// <summary>
        /// Create any fields required by the model which haven't already
        /// been defined
        /// </summary>
        private void _PrepareModel()
        {
            foreach (var pi in _userModelT.GetProperties())
            {
                // Check for ignore attribute
                var ignAttr = pi
                    .GetCustomAttributes(typeof(EditorIgnoreAttribute), true)
                    .Cast<EditorIgnoreAttribute>().FirstOrDefault();

                if (ignAttr != null && ignAttr.Ignore)
                {
                    continue;
                }

                var field = _FindField(pi.Name, "name");

                // If the field doesn't exist yet, create it
                if (field == null)
                {
                    field = new Field(pi.Name);
                    Field(field);
                }

                // Then assign the information from the model
                field.Type(pi.PropertyType);

                var err = pi.GetCustomAttributes(typeof(EditorTypeErrorAttribute), false)
                     .Cast<EditorTypeErrorAttribute>().FirstOrDefault();

                if (err != null)
                {
                    field.TypeError(err.Msg);
                }

                var name = pi.GetCustomAttributes(typeof(EditorHttpNameAttribute), false)
                     .Cast<EditorHttpNameAttribute>().FirstOrDefault();

                if (name != null)
                {
                    field.Name(name.Name);
                }

                var get = pi
                    .GetCustomAttributes(typeof(EditorGetAttribute), false)
                    .Cast<EditorGetAttribute>().FirstOrDefault();

                if (get != null)
                {
                    field.Get(get.Get);
                }

                var set = pi
                    .GetCustomAttributes(typeof(EditorSetAttribute), false)
                    .Cast<EditorSetAttribute>().FirstOrDefault();

                if (set != null)
                {
                    field.Set(set.Set);
                }
            }
        }

        /// <summary>
        /// Find a field based on a name
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="type">Name type (db or name)</param>
        /// <returns>Found field</returns>
        private Field _FindField(string name, string type)
        {
            for (int i = 0, ien = _fields.Count(); i < ien; i++)
            {
                var field = _fields[i];

                if (type == "name" && field.Name() == name)
                {
                    return field;
                }

                if (type == "db" && field.DbField() == name)
                {
                    return field;
                }
            }

            return null;
        }

        /// <summary>
        /// Get a list of table names that the host Editor instance can use
        /// </summary>
        /// <returns>List of tables</returns>
        private List<string> _ParentTables ()
        {
            var resolved = new List<string>();
            var i = 0;
            var tables = _editor.Table();
            var joins = _editor.LeftJoin();

            // Main table(s)
            for (i=0 ; i<tables.Count() ; i++)
            {
                resolved.Add(tables[i]);
            }

            // Left joined tables
            for (i=0 ; i<joins.Count() ; i++)
            {
                resolved.Add(joins[i].Table);
            }

            return resolved;
        }
    }
}
