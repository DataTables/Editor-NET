using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTables
{
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
        private string _table;
        private string _value;
        private IEnumerable<string> _label;
        private Func<Dictionary<string, object>, string> _renderer;
        private Action<Query> _where;
        private string _order;
        private int _limit=-1;

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
            var list = new List<string> {label};

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
            _label = label;

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
        /// Set the limit for the number of options returned. NOTE if you are using
        /// SQL Server, make sure you also set an `Order` option.
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
            return _order;
        }

        /// <summary>
        /// Set the order by clause for the options
        /// </summary>
        /// <param name="order">Order by SQL statement</param>
        /// <returns>Self for chaining</returns>
        public Options Order(string order)
        {
            _order = order;

            return this;
        }

        /// <summary>
        /// Get the rendering function
        /// </summary>
        /// <returns>Rendering function</returns>
        public Func<Dictionary<string, object>, string> Render()
        {
            return _renderer;
        }

        /// <summary>
        /// Set the rendering function for the option labels
        /// </summary>
        /// <param name="renderer">Rendering function. Called once for each option</param>
        /// <returns>Self for chaining</returns>
        public Options Render(Func<Dictionary<string, object>, string> renderer)
        {
            _renderer = renderer;

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
        internal List<Dictionary<string, object>> Exec(Database db)
        {
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

            var fields = new List<string>(_label) { _value };
            var q = db.Query("select")
                .Distinct(true)
                .Table(_table)
                .Get(fields)
                .Where(_where);

            if (_order != null)
            {
                // If ordering is used and the field specified isn't in the list to select,
                // then the select distinct would throw an error. So we need to add it in.
                foreach (var field in _order.Split(','))
                {
                    var col = field.ToLower().Replace(" asc", "").Replace(" desc", "");

                    if (! fields.Contains(col))
                    {
                        q.Get(col);
                    }
                }
               
                q.Order(_order);
            }

            if (_limit != -1)
            {
                q.Limit(_limit);
            }

            var rows = q
                .Exec()
                .FetchAll();

            var output = rows.Select(row => new Dictionary<string, object>
            {
                {"value", row[_value]},
                {"label", formatter(row)}
            });

            if (_order == null)
            {
                output = output.OrderBy(x => x["label"]);
            }

            return output.ToList();
        }
    }
}
