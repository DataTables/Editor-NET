using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DataTables.EditorUtil;

namespace DataTables
{
    using OptionsFunc = Func<Database, string, List<Dictionary<string, object>>>;

    /// <summary>
    /// The Options class provides a convenient method of specifying where Editor
    /// should get the list of options for a `select`, `radio` or `checkbox` field.
    /// This is normally from a table that is _left joined_ to the main table being
    /// edited, and a list of the values available from the joined table is shown to
    /// the end user to let them select from.
    ///
    /// `Options` instances are used with the `Field.Options()` method.
    /// </summary>
    public class Options
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructor
         */

        /// <summary>
        /// Create a new Options instance, to be configured by its methods.
        /// </summary>
        public Options() {}

        /// <summary>
        /// Create a new Options instance, setting basic database lookup details
        /// </summary>
        /// <param name="table">Table name (`.Table()`)</param>
        /// <param name="value">Value column name (`.Value()`)</param>
        /// <param name="label">Label column name (`.Label()`)</param>
        public Options(string table, string value, string label)
        {
            this.Table(table);
            this.Value(value);
            this.Label(label);
        }

        /// <summary>
        /// Create a new Options instance, setting a custom function (`.Fn()`).
        /// </summary>
        /// <param name="fn">Custom function used to get options</param>
        public Options(OptionsFunc fn)
        {
            this.Fn(fn);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private parameters
         */
        private bool _alwaysRefresh = true;
        private OptionsFunc _customFn;
        private bool _get = true;
        private List<string> _includes = new List<string>();
        private List<string> _label;
        private readonly List<LeftJoin> _leftJoin = new List<LeftJoin>();
        private int _limit = -1;
        private List<Dictionary<string, object>> _manualOpts = new List<Dictionary<string, object>>();
        private string _orderSql = null;
        private bool _orderLocal = true;
        private Func<Dictionary<string, object>, object> _renderer;
        private bool _searchOnly = false;
        private string _table;
        private string _value;
        private Action<Query> _where;


        /// <summary>
        /// Add a manually defined option to the list from the database. The object added should
        /// contain the same keys that are provided by the database result and the option added _IS_
        /// passed through the label renderer.
        /// </summary>
        /// <param name="Row"></param>
        /// <returns>Self for chaining</returns>
        public Options Add(Dictionary<string, object> row)
        {
            _manualOpts.Add(row);

            return this;
        }


        /// <summary>
        /// Add a manually defined option to the list from the database. Note that options added
        /// with this method will not be passed through the label rendering. The label given will be
        /// used for both the label and value as is.
        /// </summary>
        /// <param name="label">Label and value</param>
        /// <returns>Self for chaining</returns>
        public Options Add(string label)
        {
            return Add(label, label);
        }

        /// <summary>
        /// Add a manually defined option to the list from the database. Note that options added
        /// with this method will not be passed through the label rendering. The label and value
        /// given will be used as is.
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="value">Value</param>
        /// <returns>Self for chaining</returns>
        public Options Add(string label, object value)
        {
            _manualOpts.Add(new Dictionary<string, object>
            {
                {"value", value},
                {"label", label},
                {"_manual", true}
            });

            return this;
        }

        /// <summary>
        /// Add manually defined options to the list from the database. Note that options added
        /// with this method will not be passed through the label rendering. The label and value
        /// given will be used as is.
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="value">Value</param>
        /// <returns>Self for chaining</returns>
        public Options AddFromEnum<T>(bool useValueAsKey = true)
        {
            foreach (var pair in Enums.ConvertToStringDictionary<T>(useValueAsKey))
            {
                if (int.TryParse(pair.Key, out var valueInt))
                {
                    _manualOpts.Add(new Dictionary<string, object> {
                        { "value", valueInt },
                        { "label", pair.Value },
                        {"_manual", true}
                    });
                }
                else
                {
                    _manualOpts.Add(new Dictionary<string, object> {
                        { "value", pair.Key },
                        { "label", pair.Value },
                        {"_manual", true}
                    });
                }
            }

            return this;
        }

        /// <summary>
        /// Get the current alwaysRefresh flag
        /// </summary>
        /// <returns>Current value</returns>
        public bool AlwaysRefresh()
        {
            return _alwaysRefresh;
        }

        /// <summary>
        /// Set the flag to indicate that the options should always be refreshed (i.e. on get,
        /// create, edit and delete) or only on the initial data load (false).
        /// </summary>
        /// <param name="set">Flag to set the always refresh to</param>
        /// <returns>Self for chaining</returns>
        public Options AlwaysRefresh(bool set)
        {
            _alwaysRefresh = set;

            return this;
        }

        /// <summary>
        /// Get the function (if set) to get the options
        /// </summary>
        /// <returns>Custom options function</returns>
        public OptionsFunc Fn()
        {
            return _customFn;
        }

        /// <summary>
        /// Set the function used to get the options, rather than using the built in DB
        /// configuration.
        /// </summary>
        /// <param name="set"><Function to use for the custom options function/param>
        /// <returns>Self for chaining</returns>
        public Options Fn(OptionsFunc set)
        {
            _customFn = set;

            return this;
        }

        /// <summary>
        /// Get the current "Get" flag
        /// </summary>
        /// <returns>Current value</returns>
        public bool Get()
        {
            return _get;
        }

        /// <summary>
        /// Set a flag to indicate if the options from this class should be obtained or not
        /// </summary>
        /// <param name="set">Flag to set</param>
        /// <returns>Self for chaining</returns>
        public Options Get(bool set)
        {
            _get = set;

            return this;
        }

        /// <summary>
        /// Get the list of field names to include in the option objects
        /// </summary>
        /// <returns>List of columns</returns>
        public List<string> Include()
        {
            return _includes;
        }

        /// <summary>
        /// Add a column name from `Value()` and `Label()` to include in the output object for each
        /// option, in addition to the value and label.
        /// </summary>
        /// <param name="field">Column name to include</param>
        /// <returns>Self for chaining</returns>
        public Options Include(string field)
        {
            _includes.Add(field);

            return this;
        }

        /// <summary>
        /// Add columns from `Value()` and `Label()` to include in the output object for each
        /// option, in addition to the value and label.
        /// </summary>
        /// <param name="fields">Column names to include</param>
        /// <returns>Self for chaining</returns>
        public Options Include(IEnumerable<string> fields)
        {
            foreach (var field in fields)
            {
                _includes.Add(field);
            }

            return this;
        }

        /// <summary>
        /// Get the column name(s) for the options label
        /// </summary>
        /// <returns>Column name(s)</returns>
        public IEnumerable<string> Label()
        {
            return _label;
        }

        /// <summary>
        /// Set the column name for the options label
        /// </summary>
        /// <param name="label">Column name</param>
        /// <returns>Self for chaining</returns>
        public Options Label(string label)
        {
            var list = new List<string> { label };

            _label = list;

            return this;
        }

        /// <summary>
        /// Set multiple column names for the options label
        /// </summary>
        /// <param name="label">Column names</param>
        /// <returns>Self for chaining</returns>
        public Options Label(IEnumerable<string> label)
        {
            _label = label.ToList();

            return this;
        }

        /// <summary>
        /// Add a left join condition to the Options instance, allowing it to operate
        /// over multiple tables.
        /// </summary>
        /// <param name="table">Table name to do a join onto</param>
        /// <param name="field1">Field from the parent table to use as the join link</param>
        /// <param name="op">Join condition (`=`, '&lt;`, etc)</param>
        /// <param name="field2">Field from the child table to use as the join link</param>
        /// <returns>Self for chaining</returns>
        public Options LeftJoin(string table, string field1, string op = null, string field2 = null)
        {
            _leftJoin.Add(new LeftJoin(table, field1, op, field2));

            return this;
        }

        /// <summary>
        /// Get the current limit
        /// </summary>
        /// <returns>Limit</returns>
        public int Limit()
        {
            return _limit;
        }

        /// <summary>
        /// Set the limit for the number of options returned.
        /// </summary>
        /// <param name="limit">Number of records to limit to</param>
        /// <returns>Self for chaining</returns>
        public Options Limit(int limit)
        {
            _limit = limit;

            return this;
        }

        /// <summary>
        /// Get the order by clause for the options
        /// </summary>
        /// <returns>Order by string</returns>
        public string Order()
        {
            return _orderSql;
        }

        /// <summary>
        /// Set the order by SQL clause for the options when getting from
        /// the database.
        /// </summary>
        /// <param name="order">Order by SQL statement</param>
        /// <returns>Self for chaining</returns>
        public Options Order(string order)
        {
            _orderSql = order;
            _orderLocal = false;

            return this;
        }

        /// <summary>
        /// Use local ordering rather than in the SQL database.
        /// 
        /// If this option is `true` (which it is by default) the ordering will
        /// be based on the rendered output, either numerically or alphabetically
        /// based on the data returned by the renderer. If `false` no ordering
        /// will be performed and whatever is returned from the database will 
        /// be used.
        /// </summary>
        /// <param name="order">Enable local sorting</param>
        /// <returns>Self for chaining</returns>
        public Options Order(bool order)
        {
            _orderSql = null;
            _orderLocal = order;

            return this;
        }

        /// <summary>
        /// Get the rendering function
        /// </summary>
        /// <returns>Rendering function</returns>
        public Func<Dictionary<string, object>, object> Render()
        {
            return _renderer;
        }

        /// <summary>
        /// Set the rendering function for the option labels
        /// </summary>
        /// <param name="renderer">Rendering function. Called once for each option</param>
        /// <returns>Self for chaining</returns>
        public Options Render(Func<Dictionary<string, object>, object> renderer)
        {
            _renderer = renderer;

            return this;
        }

        /// <summary>
        /// Get the current SearchOnly value
        /// </summary>
        /// <returns>Flag's value</returns>
        public bool SearchOnly()
        {
            return _searchOnly;
        }

        /// <summary>
        /// Set the flag to indicate if the options should always be refreshed (i.e. on get,
        ///  create, edit and delete) or only on the initial data load (false).
        /// </summary>
        /// <param name="set">Flag to set the search only option to</param>
        /// <returns>Self for chaining</returns>
        public Options SearchOnly(bool set)
        {
            _searchOnly = set;

            return this;
        }

        /// <summary>
        /// Get the table configured to read the options from
        /// </summary>
        /// <returns>Table name</returns>
        public string Table()
        {
            return _table;
        }

        /// <summary>
        /// Set the table to read the options from
        /// </summary>
        /// <param name="table">Table name</param>
        /// <returns>Self for chaining</returns>
        public Options Table(string table)
        {
            _table = table;

            return this;
        }

        /// <summary>
        /// Get the value column name
        /// </summary>
        /// <returns>Value column name</returns>
        public string Value()
        {
            return _value;
        }

        /// <summary>
        /// Set the value column name
        /// </summary>
        /// <param name="value">Column name</param>
        /// <returns>Self for chaining</returns>
        public Options Value(string value)
        {
            _value = value;

            return this;
        }

        /// <summary>
        /// Get the WHERE function used to apply conditions to the options select
        /// </summary>
        /// <returns>Function</returns>
        public Action<Query> Where()
        {
            return _where;
        }

        /// <summary>
        /// Set a function that will be used to apply conditions to the options select
        /// </summary>
        /// <param name="where">Function that will add conditions to the query</param>
        /// <returns>Self for chaining</returns>
        public Options Where(Action<Query> where)
        {
            _where = where;

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */

        /// <summary>
        /// Execute the configuration, getting the options from the database and formatting
        /// for output.
        /// </summary>
        /// <param name="db">Database connection object</param>
        /// <returns>List of options</returns>
        internal List<Dictionary<string, object>> Exec(
            Database db,
            bool refresh,
            string search = null,
            List<string> find = null
        )
        {
		    // Local enablement
            if (_get == false) {
                return null;
            }

            // If search only, and not a search action, then just return false
            if (_searchOnly && search == null && find == null)
            {
                return null;
            }

            // Only get the options if doing a full load, or always is set
            if (refresh && !_alwaysRefresh)
            {
                return null;
            }

            if (_customFn != null)
            {
                return _customFn(db, search);
            }

            // Default formatter if one isn't provided
            var formatter = _renderer ?? (row =>
            {
                var list = new List<string>();

                foreach (var label in _label)
                {
                    if (row[label] != null)
                    {
                        list.Add(row[label].ToString());
                    }
                }

                return string.Join(" ", list);
            });

            // Get database data
            var options = ExecDb(db, find);

            // Manually added options
            foreach (var opt in _manualOpts)
            {
                options.Add(opt);
            }

            // Create the output list
            var output = new List<Dictionary<string, object>>();

            foreach (var opt in options)
            {
                var rowLabel = opt.ContainsKey("_manual")
                    ? opt["value"] as string
                    : formatter(opt) as string;
                var rowValue = opt.ContainsKey("_manual")
                    ? opt["label"]
                    : opt[_value];

                // Apply the search to the rendered label. Need to do it here rather than in SQL since
                // the label is rendered in script.
                if (
                    search == null ||
                    search == "" ||
                    rowLabel.ToLower().IndexOf(search.ToLower()) == 0
                )
                {
                    var option = new Dictionary<string, object>();

                    option.Add("label", rowLabel);
                    option.Add("value", rowValue);

                    // Add in any column that are needed for extra data (includes)
                    foreach (var inc in _includes)
                    {
                        if (opt.ContainsKey(inc))
                        {
                            option.Add(inc, opt[inc]);
                            option[inc] = opt[inc];
                        }
                    }

                    output.Add(option);
                }

                // Limit needs to be done in script space, rather than SQL, to allow for the script
                // based filtering above, and also for when using a custom function
                if (_limit != -1 && output.Count >= _limit)
                {
                    break;
                }
            }

            if (_orderLocal == true)
            {
                output.Sort((a, b) => a["label"].ToString().CompareTo(b["label"].ToString()));
            }

            return output;
        }

        /// <summary>
        /// Get the list of options form the database based on the configuration
        /// </summary>
        /// <param name="db">Database connection</param>
        /// <param name="find">Values to search for if any</param>
        /// <returns>List of found options</returns>
        internal List<Dictionary<string, object>> ExecDb(Database db, List<string> find)
        {
            var fields = new List<string>(_label);

            if (! fields.Contains(_value)) {
                fields.Add(_value);
            }

            var q = db.Query("select")
                .Distinct(true)
                .Table(_table)
                .Get(fields)
                .Where(_where)
                .LeftJoin(_leftJoin);

            if (_orderSql != null)
            {
                // If ordering is used and the field specified isn't in the list to select,
                // then the select distinct would throw an error. So we need to add it in.
                foreach (var field in _orderSql.Split(new[] { ',' }))
                {
                    var col = field.ToLower().Replace(" asc", "").Replace(" desc", "");

                    if (!fields.Contains(col))
                    {
                        q.Get(col);
                    }
                }

                q.Order(_orderSql);
            }
            else if (_orderLocal)
            {
                q.Order(_label[0] + " asc");
            }

            var rows = q
                .Exec()
                .FetchAll();

            return rows;
        }

        /// <summary>
        /// Get the objects for a set of values.
        /// </summary>
        /// <param name="db">Database connection</param>
        /// <param name="ids">IDs to get</param>
        /// <returns>List of options</returns>
        public List<Dictionary<string, object>> Find(Database db, List<string> ids)
        {
            return Exec(db, false, null, ids);
        }

        /// <summary>
        /// Do a search for data on the ousrce
        /// </summary>
        /// <param name="db">Database connection</param>
        /// <param name="term">Search term</param>
        /// <returns>List of options</returns>
        public List<Dictionary<string, object>> Search(Database db, string term)
        {
            return Exec(db, false, term);
        }
    }
}
