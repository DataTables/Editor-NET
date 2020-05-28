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
        private IEnumerable<string> _label;
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
            Console.WriteLine(89);
            return _renderer;
        }

        /// <summary>
        /// Set the rendering function for the SearchPaneOption labels
        /// </summary>
        /// <param name="renderer">Rendering function. Called once for each option</param>
        /// <returns>Self for chaining</returns>
        public SearchPaneOptions Render(Func<string, string> renderer)
        {
            Console.WriteLine(100);
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

        /// <summary>
	/// Applies the leftJoin to the query passed in as a parameter
        /// </summary>
        /// <param name="q">Query that the leftJoin is to be applied to</param>
        /// <returns>Self for chaining</returns>
        private void _PerformLeftJoin(Query q)
        {
            if (_leftJoin.Count() != 0)
            {
                foreach (var join in _leftJoin)
                {
                    q.Join(join.Table, join.Field1 + " " + join.Operator + " " + join.Field2, "LEFT");
                }
            }
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
            string _label;

            if(this._value == null){
                this._value = fieldIn.DbField();
            }
            if(this._table == null){
                this._table = editor.Table().ToString();
            }
            if(this._label == null){
                _label = this._value;
            }
            else {
                _label = this._label.First();
            }

            // Just return the label if no default renderer
            var formatter = _renderer ?? (str =>
            {
                return str;
            });

            if(leftJoinIn.Count() > 0){
                this._leftJoin = leftJoinIn;
            }

            var query = db.Query("select")
                .Table(editor.Table());
            
            if(fieldIn.Apply("get") && fieldIn.GetValue() == null){
                query.Get(this._value + " as value")
                    .Get("COUNT(*) as count")
                    .GroupBy(this._value);
            }
            // Loop over fields
            for(int i = 0; i < fields.Count(); i++)
            {
                if (http.searchPanes.ContainsKey(fields[i].Name()))
                {
		    // Apply Or where based upon searchPanes selections
                    query.Where(qu =>
                    {
                        foreach(var opt in http.searchPanes[fields[i].Name()]){
                            qu.OrWhere(fields[i].Name(), opt, "=");
                        }
                    });
                }
            }

            _PerformLeftJoin(query);
            var res = query.Exec()
                .FetchAll();

            var q = db.Query("select")
                .Distinct(true)
                .Table(editor.Table())
                .Get(_label + " as label")
                .Get(this._value + " as value")
                .Get("COUNT(*) as total")
                .GroupBy(this._value)
                .Where(this._where);

            _PerformLeftJoin(q);

            var rows = q
                .Exec()
                .FetchAll();

	    // Create output object with all of the SearchPaneOptions
            List<Dictionary<string, object>> output = new List<Dictionary<string, object>>();
            for (int i=0, ien=rows.Count() ; i<ien ; i++ ) {
                bool set = false;
                for( int j=0 ; j<res.Count() ; j ++) {
                    if(res[j]["value"].ToString() == rows[i]["value"].ToString()) {
                        output.Add(new Dictionary<string, object>{
                            {"label", formatter(rows[i]["label"].ToString())},
                            {"total", rows[i]["total"]},
                            {"value", rows[i]["value"].ToString()},
                            {"count", res[j]["count"]}
                        });
                        set = true;
                    }
                }
		// If it has not been set then there aren't any so set count to 0
                if(!set) {
                    output.Add(new Dictionary<string, object>{
                        {"label", formatter(rows[i]["label"].ToString())},
                        {"total", rows[i]["total"]},
                        {"value", rows[i]["value"].ToString()},
                        {"count", 0}
                    });
                }
                
            }

            if (_order == null)
            {
                output.Sort((a, b) => a["label"].ToString().CompareTo(b["label"].ToString()));
            }

            return output.ToList();
        }
    }
}
