using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// The SearchPaneOptions class provides a convenient method of specifying where Editor
    /// should get the list of options for SearchPanes options list.
    /// This is normally from a table that is _left joined_ to the main table being
    /// edited, and a list of the values available from the joined table is shown to
    /// the end user to let them select from.
    ///
    /// `SearchPanesOptions` instances are used with the `Field.SearchPaneOptions()` method.
    /// </summary>
    public class SearchPaneOptions
    {
        private string _table;
        private string _value;
        private IEnumerable<string> _label = new List<string>();
        private Func<string, string> _renderer;
        private Action<Query> _where;
        private string _order;
        private List<LeftJoin> _leftJoin = new List<LeftJoin>();

        /// <summary>
        /// Get the column name(s) for the options label
        /// </summary>
        /// <returns>Column name(s)</returns>
        public IEnumerable<string> Label()
        {
            return _label;
        }

        /// <summary>
        /// Set the column name for the SearchPaneOptions label
        /// </summary>
        /// <param name="label">Column name</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Label(string label)
        {
            var list = new List<string> {label};

            _label = list;

            return this;
        }

        /// <summary>
        /// Set multiple column names for the SearchPaneOptions label
        /// </summary>
        /// <param name="label">Column names</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Label(IEnumerable<string> label)
        {
            _label = label;

            return this;
        }

        /// <summary>
        /// Get the order by clause for the SearchPaneOptions
        /// </summary>
        /// <returns>Order by string</returns>
        public string Order()
        {
            return _order;
        }

        /// <summary>
        /// Set the order by clause for the SearchPaneOptions
        /// </summary>
        /// <param name="order">Order by SQL statement</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Order(string order)
        {
            _order = order;

            return this;
        }

        /// <summary>
        /// Get the rendering function
        /// </summary>
        /// <returns>Rendering function</returns>
        public Func<string, string> Render()
        {
            return _renderer;
        }

        /// <summary>
        /// Set the rendering function for the SearchPaneOption labels
        /// </summary>
        /// <param name="renderer">Rendering function. Called once for each option</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Render(Func<string, string> renderer)
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
        /// Set the table to read the SearchPaneOptions from
        /// </summary>
        /// <param name="table">Table name</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Table(string table)
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
        public SearchPaneOptions Value(string value)
        {
            _value = value;

            return this;
        }

        /// <summary>
        /// Get the WHERE function used to apply conditions to the SearchPaneOptions select
        /// </summary>
        /// <returns>Function</returns>
        public Action<Query> Where()
        {
            return _where;
        }

        /// <summary>
        /// Set a function that will be used to apply conditions to the SearchPaneOptions select
        /// </summary>
        /// <param name="where">Function that will add conditions to the query</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Where(Action<Query> where)
        {
            _where = where;

            return this;
        }

        /// <summary>
        /// Set a function that will be used to apply a leftJoin to the SearchPanes select
        /// </summary>
        /// <param name="table">String representing the table for the leftJoin</param>
        /// <param name="field1">String representing the first Field for the leftJoin</param>
        /// <param name="op">String representing the operatore for the leftJoin</param>
        /// <param name="field2">String representing the second Field for the leftJoin</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions LeftJoin(string table, string field1, string op, string field2)
        {
            _leftJoin.Add(new LeftJoin(table, field1, op, field2));

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private methods
         */

        /// <summary>
        /// Add conditions to a query for cascade
        /// </summary>
        /// <param name="entriesQuery">Query to apply the condition to</param>
        /// <param name="http">DTRequest Instance for where conditions</param>
        /// <param name="fileName">Field name being added</param>
        private void _QueryAddCondition(Query entriesQuery, DtRequest http, string fieldName, string fieldDb)
        {
        }
        

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */

        /// <summary>
        /// Execute the configuration, getting the SearchPaneOptions from the database and formatting
        /// for output.
        /// </summary>
        /// <param name="fieldIn">Field that the SearchPane options are to be found for</param>
        /// <param name="editor">Editor Instance</param>
        /// <param name="leftJoinIn">List of LeftJoins to be performed</param>
        /// <param name="http">DTRequest Instance for where conditions</param>
        /// <param name="fields">Array of all of the other fields</param>
        /// <returns>List of SearchPaneOptions</returns>
        internal List<Dictionary<string, object>> Exec(Field fieldIn, Editor editor, List<LeftJoin> leftJoinIn, DtRequest http, Field[] fields)
        {
            var db = editor.Db();
            var viewCount = http.searchPanesOptions.ViewCount;
            var viewTotal = http.searchPanesOptions.ViewTotal;
            var cascade = http.searchPanesOptions.Cascade;
            var gettingCount = false;
            Dictionary<string, object> entries = null;

    		// If the value is not yet set then set the variable to be the field name
            var value = _value ?? fieldIn.DbField();

            // If the table is not yet set then set the table variable to be the same as editor
            // This is not taking a value from the SearchPaneOptions instance as the table should be defined in value/label. This throws up errors if not.
            var table = editor.Table()[0];
            var readTable = editor.ReadTable();

            if (_table != null) {
                table = _table;
            }
            else if (readTable.Count() != 0) {
                table = readTable[0];
            }
            
    		// If the label value has not yet been set then just set it to be the same as value
            var label = _label.Count() != 0 ? _label.ElementAt(0) : value;

            // Just return the label if no default renderer
            var formatter = _renderer ?? (str =>
            {
                return str;
            });

    		// Use Editor's left joins and merge in any additional from this instance
            var join = new List<LeftJoin>(_leftJoin);

            foreach(var lfi in leftJoinIn) {
                var found = false;

                foreach(var inner in join) {
                    if (inner.Table == lfi.Table) {
                        found = true;
                    }
                }

                if (! found) {
                    join.Add(lfi);
                }
            }
            
    		// Get the data for the pane options
            var q = db.Query("select")
                .Distinct(true)
                .Table(table)
                .Get(label + " as label")
                .Get(value + " as value")
                .GroupBy(value)
                .Where(_where)
                .LeftJoin(join);

            if (viewTotal) {
                q.Get("COUNT(*) as total");
            }

            if ( _order != null ) {
                // For cases where we are ordering by a field which isn't included in the list
                // of fields to display, we need to add the ordering field, due to the
                // select distinct.
                var orderFields = _order.Split(new[] {','});

                foreach(var orderField in orderFields) {
                    var clean = orderField.ToLower();
                    clean = clean.Replace(" asc", "");
                    clean = clean.Replace(" desc", "");
                    clean = clean.Trim();

                    if (! q.Get().Contains(clean)) {
                        q.Get(clean);
                    }
                }

                q.Order(_order);
            }

            var rows = q
                .Exec()
                .FetchAll();

    		// Remove any filtering entries that don't exist in the database (values might have changed)
            if (http.searchPanes.ContainsKey(fieldIn.Name())) {
                var values = rows.Select(r => r["value"].ToString());
                var selected = http.searchPanes[fieldIn.Name()];

                http.searchPanes[fieldIn.Name()] = selected.Intersect(values).ToArray();
            }

	    	// Apply filters to cascade tables
            if (viewCount || cascade) {
                var entriesQuery = db.Query("select")
                    .Distinct(true)
                    .Table(table)
                    .LeftJoin(join);

                if (fieldIn.Apply("get") && fieldIn.GetValue() == null) {
                    gettingCount = true;
                    entriesQuery.Get(value + " as value");
                    entriesQuery.GroupBy(value);

                    // We viewTotal is enabled, we need to do a count to get the number of records,
                    // If it isn't we still need to know it exists, but don't care about the cardinality
                    if (viewCount) {
                        entriesQuery.Get("COUNT(*) as count");
                    }
                    else {
                        entriesQuery.Get("(1) as count");
                    }
                }

                // Construct the where queries based upon the options selected by the user
                for(int i = 0; i < fields.Count(); i++) {
                    var add = false;
                    var fieldName = fields[i].Name();

                    // If there is a last value set then a slightly different set of results is required for cascade
                    // That panes results are based off of the results when only considering the selections of all of the others
                    if (http.searchPanesLast != null && fieldIn.Name() == http.searchPanesLast) {
                        if (http.searchPanes.ContainsKey(fieldName) && fieldName != http.searchPanesLast) {
                           add = true;
                        }
                    }
                    else if (http.searchPanes != null && http.searchPanes.ContainsKey(fieldName)) {
                        add = true;
                    }

                    if (add) {
                        entriesQuery.Where(qu => {
                            for(int j =0; j < http.searchPanes[fieldName].Count(); j++) {
                                qu.OrWhere(
                                    fields[i].DbField(),
                                    http.searchPanes_null.ContainsKey(fieldName) && http.searchPanes_null[fieldName][j] ?
                                        null :
                                        http.searchPanes[fieldName][j],
                                    "="
                                );
                            }
                        });
                    }
                }

                var entriesRows = entriesQuery
                    .Exec()
                    .FetchAll();

    			// Key by the value for fast lookup
                entries = new Dictionary<string, object>();

                foreach(var entry in entriesRows) {
                    entries.Add(entry["value"].ToString(), entry);
                }
            }

            var output = new List<Dictionary<string, object>>();

            foreach(var row in rows) {
                var val = row["value"].ToString();
                Int64? total = row.ContainsKey("total") ? (Int64?)row["total"] : null;
                Int64? count = total;

                if (entries != null) {
                    count = 0;

                    if (entries.ContainsKey(val) && gettingCount) {
                        var diction = (Dictionary<string, object>)entries[val];

                       count = (Int64?)diction["count"];

                        // For when viewCount is enabled and viewTotal is not
                        // the total needs to be the same as the count!
                        if (total == null) {
                            total = count;
                        }
                    }
                }

                output.Add(new Dictionary<string, object>{
                    {"label", formatter(
                        (row["label"] is DBNull) ? null : row["label"].ToString()
                    )},
                    {"total", total},
                    {"value", row["value"] is DBNull ? null : val},
                    {"count", count}
                });
            }

		    // Only sort if there was no SQL order field
            if (_order == null) {
                string emptyStringA = "";
                string emptyStringB = "";

                output.Sort((a, b) => (a["label"] == null && b["label"] == null) ?
                    emptyStringA.CompareTo(emptyStringB) :
                    (a["label"] == null) ?
                        emptyStringA.CompareTo(b["label"].ToString()) :
                        (b["label"] == null) ?
                            a["label"].ToString().CompareTo(emptyStringB) :
                            a["label"].ToString().CompareTo(b["label"].ToString())
                );
            }

            return output;
        }
    }
}
