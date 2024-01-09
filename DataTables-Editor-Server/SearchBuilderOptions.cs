using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// The SearchBuilderOptions class provides a convenient method of specifying where Editor
    /// should get the list of options for SearchBuilder options list.
    /// This is normally from a table that is _left joined_ to the main table being
    /// edited, and a list of the values available from the joined table is shown to
    /// the end user to let them select from.
    ///
    /// `SearchBuilderOptions` instances are used with the `Field.SearchBuilderOptions()` method.
    /// </summary>
    public class SearchBuilderOptions
    {
        private string _table;
        private string _value;
        private IEnumerable<string> _label;
        private Func<string, string> _renderer;
        private Action<Query> _where;
        private string _order;
        private List<LeftJoin> _leftJoin = new List<LeftJoin>();
        private Dictionary<string, string> _fromEnum = new Dictionary<string, string>();

        /// <summary>
        /// Get the column name(s) for the options label
        /// </summary>
        /// <returns>Column name(s)</returns>
        public IEnumerable<string> Label()
        {
            return _label;
        }

        /// <summary>
        /// Set the column name for the SearchBuilderOptions label
        /// </summary>
        /// <param name="label">Column name</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions Label(string label)
        {
            var list = new List<string> {label};

            _label = list;

            return this;
        }

        /// <summary>
        /// Set multiple column names for the SearchBuilderOptions label
        /// </summary>
        /// <param name="label">Column names</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions Label(IEnumerable<string> label)
        {
            _label = label;

            return this;
        }

        /// <summary>
        /// Get the order by clause for the SearchBuilderOptions
        /// </summary>
        /// <returns>Order by string</returns>
        public string Order()
        {
            return _order;
        }

        /// <summary>
        /// Set the order by clause for the SearchBuilderOptions
        /// </summary>
        /// <param name="order">Order by SQL statement</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions Order(string order)
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
        /// Set the rendering function for the SearchBuilderOption labels
        /// </summary>
        /// <param name="renderer">Rendering function. Called once for each option</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions Render(Func<string, string> renderer)
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
        /// Set the table to read the SearchBuilderOptions from
        /// </summary>
        /// <param name="table">Table name</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions Table(string table)
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
        public SearchBuilderOptions Value(string value)
        {
            _value = value;

            return this;
        }

        /// <summary>
        /// Get the WHERE function used to apply conditions to the SearchBuilderOptions select
        /// </summary>
        /// <returns>Function</returns>
        public Action<Query> Where()
        {
            return _where;
        }

        /// <summary>
        /// Set a function that will be used to apply conditions to the SearchBuilderOptions select
        /// </summary>
        /// <param name="where">Function that will add conditions to the query</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions Where(Action<Query> where)
        {
            _where = where;

            return this;
        }

        /// <summary>
        /// Set a function that will be used to apply a leftJoin to the SearchBuilder select
        /// </summary>
        /// <param name="table">String representing the table for the leftJoin</param>
        /// <param name="field1">String representing the first Field for the leftJoin</param>
        /// <param name="op">String representing the operatore for the leftJoin</param>
        /// <param name="field2">String representing the second Field for the leftJoin</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions LeftJoin(string table, string field1, string op, string field2)
        {
            _leftJoin.Add(new LeftJoin(table, field1, op, field2));

            return this;
        }

        /// <summary>
        /// Set a function that will be used to apply an Enum to the SearchBuilder select
        /// </summary>
        /// <param name="useValueAsKey">Boolean to use the enum value as the key (default true)</param>
        /// <returns>Self for chaining</returns>
        public SearchBuilderOptions FromEnum<T>(bool useValueAsKey = true)
        {
            _fromEnum = Enums.ConvertToStringDictionary<T>(useValueAsKey);

            return this;
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */
        
        /// <summary>
        /// Execute the configuration, getting the SearchBuilderOptions from the database and formatting
        /// for output.
        /// </summary>
        /// <param name="fieldIn">Field that the SearchBuilder options are to be found for</param>
        /// <param name="editor">Editor Instance</param>
        /// <param name="leftJoinIn">List of LeftJoins to be performed</param>
        /// <param name="http">DTRequest Instance for where conditions</param>
        /// <param name="fields">Array of all of the other fields</param>
        /// <returns>List of SearchBuilderOptions</returns>
        internal List<Dictionary<string, object>> Exec(Field fieldIn, Editor editor, List<LeftJoin> leftJoinIn, DtRequest http, Field[] fields)
        {
            var db = editor.Db();
            string _label;

            if(this._value == null){
                this._value = fieldIn.DbField();
            }

            if(this._table == null){
                var readTable = editor.ReadTable();
                if(readTable.Count() == 0) {
                    this._table = editor.Table()[0].ToString();
                }
                else {
                    this._table = readTable[0];
                }
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
                .Table(this._table)
                .LeftJoin(_leftJoin);
            
            if(fieldIn.Apply("get") && fieldIn.GetValue() == null){
                query
                    .Get(this._value + " as value")
                    .Get(_label + " as label")
                    .GroupBy(this._value);
            }

            var res = query.Exec()
                .FetchAll();

            // Replace labels from database with enum names, fall back on database values 
            if (_fromEnum.Count > 0) {
                foreach (var row in res) {
                    row["label"] = _fromEnum[row["label"].ToString()] ?? row["label"].ToString();
                }
            }

    	    // Create output object with all of the SearchBuilderOptions
            List<Dictionary<string, object>> output = new List<Dictionary<string, object>>();
            for (int i=0, ien=res.Count() ; i<ien ; i++ ) {
                output.Add(new Dictionary<string, object>{
                    {"label", formatter(
                        (res[i]["label"] is DBNull) ? null : res[i]["label"].ToString()
                    )},
                    {"value", res[i]["value"] is DBNull ? null : res[i]["value"].ToString()}
                });
            }

            if (_order == null)
            {
                string emptyStringa = "";
                string emptyStringb = "";
                output.Sort((a, b) => (a["label"] == null && b["label"] == null) ?
                    emptyStringa.CompareTo(emptyStringb) :
                    (a["label"] == null) ?
                        emptyStringa.CompareTo(b["label"].ToString()) :
                        (b["label"] == null) ?
                            a["label"].ToString().CompareTo(emptyStringb) :
                            a["label"].ToString().CompareTo(b["label"].ToString())
                );
            }

            return output.ToList();
        }
    }
}
