// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Field class which defines how individual fields for Editor
// </summary>
using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DataTables.EditorUtil;
using System.Web;

namespace DataTables
{
    using OptionsFunc = Func<Database, string, List<Dictionary<string, object>>>;

    /// <summary>
    /// Field definitions for the DataTables Editor.
    ///
    /// Each Database column that is used with Editor can be described with this 
    /// Field method (both for Editor and Join instances). It basically tells
    /// Editor what table column to use, how to format the data and if you want
    /// to read and/or write this column.
    /// </summary>
    public class Field
    {

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Statics
         */

        /// <summary>
        /// Set options for the <code>Set()</code> method, controlling when this
        /// field's value is set on the database
        /// </summary>
        public enum SetType
        {
            /// <summary>
            /// Never set this field's value
            /// </summary>
            None,

            /// <summary>
            /// Set the value on both create and edit actions
            /// </summary>
            Both,

            /// <summary>
            /// Set the value on only the create action
            /// </summary>
            Create,

            /// <summary>
            /// Set the value on only the edit action
            /// </summary>
            Edit
        };


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructor
         */

        /// <summary>
        /// Create a new Field instance - common db name and http name
        /// </summary>
        /// <param name="dbField">Database name for the field. This is also used as the HTTP name for the field</param>
        public Field(string dbField)
        {
            Name(dbField);
            DbField(dbField);
        }

        /// <summary>
        /// Create a new Field instance - different db and http names
        /// </summary>
        /// <param name="dbField">Database name for the field</param>
        /// <param name="name">HTTP name (JSON data and form submit)</param>
        public Field(string dbField, string name)
        {
            Name(name);
            DbField(dbField);
        }

        /// <summary>
        /// Create a new Field instance - common db name and http name with type specified
        /// </summary>
        /// <param name="dbField">Database name for the field. This is also used as the HTTP name for the field</param>
        /// <param name="type">Type that the value should take</param>
        /// <param name="typeError">Error message if the field's value cannot be cast to the given type</param>
        public Field(string dbField = null, Type type = null, string typeError = null)
        {
            if (dbField != null)
            {
                Name(dbField);
                DbField(dbField);
            }

            if (type != null)
            {
                Type(type);
            }

            if (typeError != null)
            {
                TypeError(typeError);
            }
        }


        /// <summary>
        /// Create a new Field instance - different db and http names with type specified
        /// </summary>
        /// <param name="dbField">Database name for the field. This is also used as the HTTP name for the field</param>
        /// <param name="name">HTTP name (JSON data and form submit)</param>
        /// <param name="type">Type that the value should take</param>
        /// <param name="typeError">Error message if the field's value cannot be cast to the given type</param>
        public Field(string dbField, string name = null, Type type = null, string typeError = null)
        {
            if (dbField != null && name == null)
            {
                Name(dbField);
                DbField(dbField);
            }
            else
            {
                Name(name);
                DbField(dbField);
            }

            if (type != null)
            {
                Type(type);
            }

            if (typeError != null)
            {
                TypeError(typeError);
            }
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private parameters
         */
        private string _dbField;
        private bool _get = true;
        private Func<object, Dictionary<string, object>, object> _getFormatter;
        private dynamic _getValue;
        private string _name;
        private Type _type = typeof(string);
        private DbType? _dbType = null;
        private string _typeError = ""; // No longer used - deprecated
        private SetType _set = SetType.Both;
        private Func<object, Dictionary<string, object>, object> _setFormatter;
        private dynamic _setValue;
        private readonly List<Func<object, Dictionary<string, object>, ValidationHost, string>> _validators =
            new List<Func<object, Dictionary<string, object>, ValidationHost, string>>();
        private Options _opts;
        private SearchPaneOptions _spOpts;
        private SearchBuilderOptions _sbOpts;
        private Func<Database, Editor, List<Dictionary<string, object>>> _spOptsFn;
        private Func<Database, Editor, List<Dictionary<string, object>>> _sbOptsFn;
        private Upload _upload;
        private Func<string, string> _xss;
        private bool _xssFormat = true;


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Set the DB field name.
        /// </summary>
        /// <returns>Database field name</returns>
        public string DbField()
        {
            return _dbField;
        }

        /// <summary>
        /// Set the DB field name.
        /// 
        /// Note that when used as a setter, an alias can be given for the field
        /// using the SQL `as` keyword - for example: `firstName as name`. In this
        /// situation the dbField is set to the field name before the `as`, and the
        /// field's name (`name()`) is set to the name after the ` as `.
        ///
        /// As a result of this, the following constructs have identical
        /// functionality:
        ///
        /// * `.field.DbField( 'firstName as name' );`
        /// * `.field.DbField( 'firstName', 'name' );`
        /// </summary>
        /// <param name="field">Database field name</param>
        /// <returns>Self for chaining</returns>
        public Field DbField(string field)
        {
            if (field.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] a = Regex.Split(field, " as ", RegexOptions.IgnoreCase);
                _dbField = a[0].Trim();
                _name = a[1].Trim();
            }
            else
            {
                _dbField = field;
            }

            return this;
        }

        public DbType? DbType()
        {
            return _dbType;
        }

        public Field DbType(DbType? type)
        {
            _dbType = type;
            return this;
        }

        /// <summary>
        /// Get the 'Get' flag for the field.
        /// </summary>
        /// <returns>Get flag</returns>
        public bool Get()
        {
            return _get;
        }

        /// <summary>
        /// Set the 'Get' flag of the field.
        ///
        /// A field can be marked as write only by setting the Get property to false
        /// </summary>
        /// <param name="get">Flag value to set</param>
        /// <returns>Self for chaining</returns>
        public Field Get(bool get)
        {
            _get = get;
            return this;
        }

        /// <summary>
        /// Get formatter for the field's data.
        ///
        /// When the data has been retrieved from the server, it can be passed through
        /// a formatter here, which will manipulate (format) the data as required. This
        /// can be useful when, for example, working with dates and a particular format
        /// is required on the client-side.
        ///
        /// Editor has a number of formatters available with the <code>Format</code> class
        /// which can be used directly with this method.
        /// </summary>
        /// <param name="fn">Get formatter that will transform the db value into the http value</param>
        /// <returns>Self for chaining</returns>
        public Field GetFormatter(Func<object, Dictionary<string, object>, object> fn)
        {
            _getFormatter = fn;

            return this;
        }

        /// <summary>
        /// Get the 'Get' value for the field
        /// </summary>
        /// <returns>Get value</returns>
        public dynamic GetValue()
        {
            return _getValue;
        }

        /// <summary>
        /// Set a "Get" value. When set this value is used to send to the
        /// client-side, regardless of what value is held by the database.
        ///  (if this field even has a database value!)
        /// </summary>
        /// <param name="val">Value to set for "Get"</param>
        /// <returns>Self for chaining</returns>
        public Field GetValue(object val)
        {
            _getValue = val;
            return this;
        }

        /// <summary>
        /// Set a "Get" delegate. When set, the delegate given here is executed
        /// when the data for the field is requested and the value returned is
        /// send to the client-side, regardless of what value is held by the
        /// database (if this field even has a database value!)
        /// </summary>
        /// <param name="val">Delegate to set for "Get"</param>
        /// <returns>Self for chaining</returns>
        public Field GetValue(Func<object> val)
        {
            _getValue = val;
            return this;
        }

        /// <summary>
        /// Get the HTTP / JSON name for the field.
        ///  
        /// The name is typically the same as the `DbField` name, since it makes things
        /// less confusing(!), but it is possible to set a different name for the data
        /// which is used in the JSON returned to DataTables in a 'get' operation and
        /// the field name used in a 'set' operation.
        /// </summary>
        /// <returns>Field HTTP name</returns>
        public string Name()
        {
            return _name;
        }

        /// <summary>
        /// Set the HTTP / JSON name for the field.
        /// 
        /// The name is typically the same as the `DbField` name, since it makes things
        /// less confusing(!), but it is possible to set a different name for the data
        /// which is used in the JSON returned to DataTables in a 'get' operation and
        /// the field name used in a 'set' operation.
        /// </summary>
        /// <param name="name">Name to set</param>
        /// <returns>Self for chaining</returns>
        public Field Name(string name)
        {
            _name = name;

            return this;
        }

        /// <summary>
        /// Get the Options object configured for this field
        /// </summary>
        /// <returns>Options object</returns>
        public Options Options()
        {
            return _opts;
        }

        /// <summary>
        /// Set a function that will retrieve a list of values that can be used
        /// for the options list in radio, select and checkbox inputs from the
        /// database for this field.
        /// </summary>
        /// <param name="fn">Delegate that will return a list of options</param>
        /// <returns>Self for chaining</returns>
        public Field Options(OptionsFunc fn)
        {
            _opts = new Options().Fn(fn);

            return this;
        }

        /// <summary>
        /// Set the options for this field using an Options instance
        /// </summary>
        /// <param name="opts">Configured options object</param>
        /// <returns>Self for chaining</returns>
        public Field Options(Options opts)
        {
            _opts = opts;

            return this;
        }

        /// <summary>
        /// Provide database information for where to get a list of values that
        /// can be used for the options list in radio, select and checkbox
        /// inputs from the database for this field.
        ///
        /// Note that this is for simple cases only. For more complex operations
        /// use the delegate overload.
        /// </summary>
        /// <param name="table">Table name to read the options from</param>
        /// <param name="value">Column name to read the option values from</param>
        /// <param name="label">Column name to read the label values from</param>
        /// <param name="condition">Function that will using the Query class passed in to apply a condtion</param>
        /// <param name="format">Formatting function (called for every option)</param>
        /// <returns>Self for chaining</returns>
        public Field Options(string table, string value, string label, Action<Query> condition = null, Func<Dictionary<string, object>, string> format = null)
        {
            var opts = new Options()
                .Table(table)
                .Value(value)
                .Label(label);

            if (condition != null)
            {
                opts.Where(condition);
            }

            if (format != null)
            {
                opts.Render(format);
            }

            _opts = opts;

            return this;
        }

        /// <summary>
        /// Provide database information for where to get a list of values that
        /// can be used for the options list in radio, select and checkbox
        /// inputs from the database for this field.
        ///
        /// Note that this is for simple cases only. For more complex operations
        /// use the delegate overload.
        /// </summary>
        /// <param name="table">Table name to read the options from</param>
        /// <param name="value">Column name to read the option values from</param>
        /// <param name="label">Column name to read the label values from</param>
        /// <param name="condition">Function that will using the Query class passed in to apply a condtion</param>
        /// <param name="format">Formatting function (called for every option)</param>
        /// <returns>Self for chaining</returns>
        public Field Options(string table, string value, IEnumerable<string> label, Action<Query> condition = null, Func<Dictionary<string, object>, string> format = null)
        {
            var opts = new Options()
                .Table(table)
                .Value(value)
                .Label(label);

            if (condition != null)
            {
                opts.Where(condition);
            }

            if (format != null)
            {
                opts.Render(format);
            }

            _opts = opts;

            return this;
        }

        /// <summary>
        /// Get the SearchBuilderOptions object configured for this field
        /// </summary>
        /// <returns>SearchBuilderOptions object</returns>
        public SearchBuilderOptions SearchBuilderOptions() {
            return _sbOpts;
        }

        /// <summary>
        /// Set a function that will retrieve a list of values that can be used
        /// for the SearchBuilderOptions list in SearchBuilders for this field.
        /// </summary>
        /// <param name="fn">Delegate that will return a list of SearchBuilder options</param>
        /// <returns>Self for chaining</returns>
        public Field SearchBuilderOptions(Func<object, object, List<Dictionary<string, object>>> fn){
            _sbOpts = null;
            _sbOptsFn = fn;

            return this;
        }

        /// <summary>
        /// Set the SearchBuilderOptions for this field using a SearchBuilderOptions instance
        /// </summary>
        /// <param name="opts">Configured SearchBuilderOptions object</param>
        /// <returns>Self for chaining</returns>
        public Field SearchBuilderOptions(SearchBuilderOptions sbOpts){
            _sbOpts = sbOpts;
            _sbOptsFn = null;

            return this;
        }
        
        /// <summary>
        /// Get the SearchPaneOptions object configured for this field
        /// </summary>
        /// <returns>SearchPaneOptions object</returns>
        public SearchPaneOptions SearchPaneOptions() {
            return _spOpts;
        }

        /// <summary>
        /// Set a function that will retrieve a list of values that can be used
        /// for the SearchPaneOptions list in SearchPanes for this field.
        /// </summary>
        /// <param name="fn">Delegate that will return a list of SearchPane options</param>
        /// <returns>Self for chaining</returns>
        public Field SearchPaneOptions(Func<object, object, List<Dictionary<string, object>>> fn){
            _spOpts = null;
            _spOptsFn = fn;

            return this;
        }

        /// <summary>
        /// Set the SearchPaneOptions for this field using a SearchPaneOptions instance
        /// </summary>
        /// <param name="opts">Configured SearchPaneOptions object</param>
        /// <returns>Self for chaining</returns>
        public Field SearchPaneOptions(SearchPaneOptions spOpts){
            _spOpts = spOpts;
            _spOptsFn = null;

            return this;
        }

        /// <summary>
        /// Get the "Set" flag for this field
        /// </summary>
        /// <returns>Set flag</returns>
        public SetType Set()
        {
            return _set;
        }

        /// <summary>
        /// Set the "Set" flag for this field.
        ///
        /// A field can be marked as read only using this option, to be set only
        /// during an create or edit action or to be set during both actions. This
        /// provides the ability to have fields that are only set when a new row is
        /// created (for example a "created" time stamp).
        ///
        /// For more control, use the `SetType` overload.
        /// </summary>
        /// <param name="set">Set flag</param>
        /// <returns>Self for chaining</returns>
        public Field Set(bool set)
        {
            Set(set ? SetType.Both : SetType.None);
            return this;
        }

        /// <summary>
        /// Set the "Set" flag for this field.
        ///
        /// A field can be marked to be set on create, edit, both or none using
        /// this method, providing the ability, for example, to write to
        /// `created` and `updated` datetime columns as appropriate.
        /// </summary>
        /// <param name="set">Set flag</param>
        /// <returns>Self for chaining</returns>
        public Field Set(SetType set)
        {
            _set = set;
            return this;
        }

        /// <summary>
        /// Set formatter for the field's data.
        ///
        /// When the data has been retrieved from the server, it can be passed through
        /// a formatter here, which will manipulate (format) the data as required. This
        /// can be useful when, for example, working with dates and a particular format
        /// is required on the client-side.
        ///
        /// Editor has a number of formatters available with the <code>Format</code> class
        /// which can be used directly with this method.
        /// </summary>
        /// <param name="fn">Get formatter delegate</param>
        /// <returns>Self for chaining</returns>
        public Field SetFormatter(Func<object, Dictionary<string, object>, object> fn)
        {
            _setFormatter = fn;

            return this;
        }

        /// <summary>
        /// Retrieve the "Set" value for the field
        /// </summary>
        /// <returns>"Set" value</returns>
        public dynamic SetValue()
        {
            return _setValue;
        }

        /// <summary>
        /// Set a "Set" value. When set this value is used to write to the
        /// database regardless of what data is sent from the client-side
        /// (if the parameter was even sent!).
        /// </summary>
        /// <param name="val">Value to set for "Set"</param>
        /// <returns>Self for chaining</returns>
        public Field SetValue(object val)
        {
            _setValue = val;
            return this;
        }

        /// <summary>
        /// Set a "Set" delegate. When set, the delegate given here is executed
        /// when the data for the field is to be written to the database and the
        /// value returned is used, regardless of what is sent by the client-side
        /// (if the parameter was even sent!).
        /// </summary>
        /// <param name="val">Delegate to set for "Set"</param>
        /// <returns>Self for chaining</returns>
        public Field SetValue(Func<object> val)
        {
            _setValue = val;
            return this;
        }

        /// <summary>
        /// Get the field type
        /// </summary>
        /// <returns>Type</returns>
        public Type Type()
        {
            return _type;
        }

        /// <summary>
        /// Set the data type for the field's values
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns>Self for chaining</returns>
        public Field Type(Type t)
        {
            _type = t.ToString().Contains("Nullable") ?
                Nullable.GetUnderlyingType(t) :
                t;

            return this;
        }

        /// <summary>
        /// Get the type error message
        /// </summary>
        /// <returns>Type error message</returns>
        public string TypeError()
        {
            return _typeError;
        }

        /// <summary>
        /// If the value retrieved from the database can't be cast to the type
        /// given, this is the error message that will be given.
        /// </summary>
        /// <param name="err">Error message</param>
        /// <returns>Self for chaining</returns>
        public Field TypeError(string err)
        {
            _typeError = err;
            return this;
        }

        /// <summary>
        /// Get the Upload instance for this field
        /// </summary>
        /// <returns>Upload instance</returns>
        public Upload Upload()
        {
            return _upload;
        }

        /// <summary>
        /// Set the upload instance for this field
        /// </summary>
        /// <param name="upload">Upload instance</param>
        /// <returns>Self for chaining</returns>
        public Field Upload(Upload upload)
        {
            _upload = upload;
            return this;
        }

        /// <summary>
        /// Set the 'validator' of the field.
        ///
        /// The validator can be used to check if any abstract piece of data is valid
        /// or not according to the given rules of the validation function used.
        ///
        /// Multiple validation options can be applied to a field instance by calling
        /// this method multiple times. For example, it would be possible to have a
        /// 'Required' validation and a 'MaxLength' validation with multiple calls.
        /// 
        /// Editor has a number of validation available with the <code>Validation</code> class
        /// which can be used directly with this method.
        /// </summary>
        /// <param name="fn">Validation method</param>
        /// <returns>Self for chaining</returns>
        public Field Validator(Func<object, Dictionary<string, object>, ValidationHost, string> fn)
        {
            _validators.Add(fn);

            return this;
        }

        /// <summary>
        /// Set a formatting method that will be used for XSS checking / removal.
        /// This should be a function that takes a single argument (the value to be
        /// cleaned) and returns the cleaned value.
        /// 
        /// Editor will use the Microsoft security library's `Encoder.HtmlEncode` 
        /// method by default for this operation, which is built into the software
        /// and no additional configuration is required, but a custom function can
        /// be used if you wish to use a different formatter such as HtmlSanitizer.
        /// 
        /// If you wish to disable this option (which you would only do if you are
        /// absolutely confident that your validation will pick up on any XSS inputs)
        /// simply pass in 'false' or provide a closure function that returns the
        /// value given to the function. This is _not_ recommended.
        /// </summary>
        /// <param name="fn">Xss formatting method</param>
        /// <returns>Self for chaining</returns>
        public Field Xss(Func<string, string> fn)
        {
            _xss = fn;

            return this;
        }

        /// <summary>
        /// Option to quickly disable XSS formatting.
        /// </summary>
        /// <param name="flag">Enable / disable XSS</param>
        /// <returns>Self for chaining</returns>
        public Field Xss(Boolean flag)
        {
            _xssFormat = flag;

            return this;
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */

        /// <summary>
        /// Check to see if a field should be used for a particular action (get or set).
        ///
        /// Called by the Editor / Join class instances - not expected for general
        /// consumption - internal.
        /// </summary>
        /// <param name="action">Direction that the data is travelling  - 'get' is reading DB data, `create` and `edit` for writing to the DB</param>
        /// <param name="data">Data submitted from the client-side when setting.</param>
        /// <returns>true if the field should be used in the get / set.</returns>
        internal bool Apply(string action, Dictionary<string, object> data = null)
        {
            if (action == "get")
            {
                // Get action - can we get this field
                return _get;
            }

            // Set - Note that validation must be done on input data before we get here.
            // Create or edit action, are we configured to use this field
            if (action == "create" && (_set == SetType.None || _set == SetType.Edit))
            {
                return false;
            }

            if (action == "edit" && (_set == SetType.None || _set == SetType.Create))
            {
                return false;
            }

            // Check it was in the submitted data. If not, then not required
            // (validation would have failed if it was) and therefore we don't
            // Set it. Check for a value as well, as it can come from another
            // source
            if (_setValue == null && !NestedData.InData(Name(), data))
            {
                return false;
            }

            // In the data set, so use it
            return true;
        }

        /// <summary>
        /// Execute the sbOpts to get the list of SearchBuilderOptions to return to the client-
        /// side
        /// </summary>
        /// <param name="field">Field instance</param>
        /// <param name="editor">Editor instance</param>
        /// <param name="leftJoin">List of LeftJoins instance</param>
        /// <param name="fields">Field[] instance</param>
        /// <param name="http">DtRequest instance</param>
        /// <returns>List of SearchBuilderOptions</returns>
        internal List<Dictionary<string, object>> SearchBuilderOptionsExec(Field field, Editor editor, List<LeftJoin> leftJoin, Field[] fields, DtRequest http) {
            if (_sbOptsFn != null){
                return _sbOptsFn(editor.Db(), editor);
            }
            if(_sbOpts != null) {
                return _sbOpts.Exec(field, editor, leftJoin, http, fields);
            }
            return null;
        }

        /// <summary>
        /// Execute the spOpts to get the list of SearchPaneOptions to return to the client-
        /// side
        /// </summary>
        /// <param name="field">Field instance</param>
        /// <param name="editor">Editor instance</param>
        /// <param name="leftJoin">List of LeftJoins instance</param>
        /// <param name="fields">Field[] instance</param>
        /// <param name="http">DtRequest instance</param>
        /// <returns>List of SearchPaneOptions</returns>
        internal List<Dictionary<string, object>> SearchPaneOptionsExec(Field field, Editor editor, List<LeftJoin> leftJoin, Field[] fields, DtRequest http) {
            if (_spOptsFn != null){
                return _spOptsFn(editor.Db(), editor);
            }
            if(_spOpts != null) {
                return _spOpts.Exec(field, editor, leftJoin, http, fields);
            }
            return null;
        }

        /// <summary>
        /// Get the value of the field, taking into account if it is coming from the
        /// DB or from a POST. If formatting has been specified for this field, it
        /// will be applied here.
        /// </summary>
        /// <param name="direction">Direction that the data is travelling  - 'get' is reading DB data, `create` and `edit` for writing to the DB</param>
        /// <param name="data">Data submitted from the client-side when setting.</param>
        /// <returns>Value for the field</returns>
        internal object Val(string direction, Dictionary<string, Object> data)
        {
            object val;

            if (direction == "get")
            {
                // Use data from the database, so the db name
                if (_getValue != null)
                {
                    val = _GetAssignedValue(_getValue);
                }
                else
                {
                    val = data.ContainsKey(_dbField) ?
                        data[_dbField] :
                        null;
                }

                return _Format(val, data, _getFormatter);
            }

            // Use data from setting from the POST / GET data, so use the name
            val = _setValue != null ?
                _GetAssignedValue(_setValue) :
                NestedData.ReadProp(Name(), data);

            // XSS prevention
            if (val is string && _xssFormat)
            {
                val = XssSafety((string)val);
            }

            return _Format(val, data, _setFormatter);
        }

        /// <summary>
        /// Check the validity of the field based on the data submitted. Note that
        /// this validation is performed on the wire data - i.e. that which is
        /// submitted, before any setFormatter is run
        /// </summary>
        /// <param name="data">Data from HTTP to check</param>
        /// <param name="editor">Editor instance</param>
        /// <param name="id">Row id for the row being edited</param>
        /// <returns>`null` if valid, or error message string if not valid</returns>
        internal string Validate(Dictionary<string, object> data, Editor editor, string id = null)
        {
            if (_validators == null)
            {
                return null;
            }

            var val = NestedData.ReadProp(Name(), data);
            var processData = editor.InData();
            var host = new ValidationHost
            {
                Action = processData.Action,
                Id = id,
                Field = this,
                Editor = editor,
                Db = editor.Db()
            };

            foreach (var validator in _validators)
            {
                var res = validator(val, data, host);

                if (res != null)
                {
                    return res;
                }
            }

            return null;
        }

        /// <summary>
        /// Write the value for this field to the output array for a read operation
        /// </summary>
        /// <param name="outData">Row output data (to the JSON)</param>
        /// <param name="srcData">Row input data (raw, from the database)</param>
        internal void Write(Dictionary<string, object> outData, Dictionary<string, object> srcData)
        {
            NestedData.WriteProp(outData, Name(), Val("get", srcData), _type);
        }

        /// <summary>
        /// Perform XSS encoding
        /// </summary>
        /// <param name="val">Value to be encoded</param>
        /// <returns></returns>
        internal string XssSafety(string val)
        {
            if ( _xss != null )
            {
                return _xss(val);
            }

            return HttpUtility.HtmlEncode( val );
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private methods
         */

        /// <summary>
        /// Apply a formatter to data. The caller will decide what formatter to apply
        /// (get or set)
        /// </summary>
        /// <param name="val">Value to be formatted</param>
        /// <param name="data">Full row data</param>
        /// <param name="formatter">Formatting function to be called</param>
        /// <returns>Formatted value</returns>
        private object _Format(object val, Dictionary<string, object> data, Func<object, Dictionary<string, object>, dynamic> formatter)
        {
            return formatter != null ?
                formatter(val, data) :
                val;
        }

        /// <summary>
        /// Get the value from `_[gs]etValue` - taking into account if it is callable
        /// function or not
        /// </summary>
        /// <param name="val">Value to be evaluated</param>
        /// <returns>Value assigned, or returned from the function</returns>
        private object _GetAssignedValue(object val)
        {
            return val is Func<object> ?
                ((Func<object>)val)() :
                val;
        }
    }
}
