// <summary>
// Column class for reading tables
// </summary>
using System;
using System.Collections.Generic;
using System.Data;

namespace DataTables
{
    using OptionsFunc = Func<Database, string, List<Dictionary<string, object>>>;

    /// <summary>
    /// Column configuration object. This is used to define how a column should
    /// be read from a database table.
    ///
    /// This class is largely a proxy to `Field`, exposing only the read aspects
    /// of the class, and not being writable.
    /// </summary>
    public class Column
    {
        /// <summary>
        /// The field instance that this instance acts as a proxy for.
        /// </summary>
        private Field _field;

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructors
         */

        /// <summary>
        /// Create a new Column instance - common db name and http name
        /// </summary>
        /// <param name="dbField">Database name for the column. This is also used as the HTTP name for the column</param>
        public Column(string dbField)
        {
            _field = new Field(dbField);
            _field.Set(false);
        }

        /// <summary>
        /// Create a new Column instance - different db and http names
        /// </summary>
        /// <param name="dbField">Database name for the column</param>
        /// <param name="name">HTTP name (JSON data and form submit)</param>
        public Column(string dbField, string name)
        {
            _field = new Field(dbField, name);
            _field.Set(false);
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Get the options class for the options to get for ColumnControl
        /// </summary>
        /// <returns>Options or null if not set</returns>
        public Options ColumnControl()
        {
            return _field.ColumnControl();
        }

        /// <summary>
        /// Set the options class for the options to get for ColumnControl
        /// </summary>
        /// <param name="opts">Options configuration for ColumnControl</param>
        /// <returns>Self for chaining</returns>
        public Column ColumnControl(Options opts)
        {
            _field.ColumnControl(opts);

            return this;
        }

        /// <summary>
        /// Set the DB field name.
        /// </summary>
        /// <returns>Database field name</returns>
        public string DbField()
        {
            return _field.DbField();
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
        public Column DbField(string field)
        {
            _field.DbField(field);

            return this;
        }

        /// <summary>
        /// Get the database type for the column
        /// </summary>
        /// <returns>The DB type</returns>
        public DbType? DbType()
        {
            return _field.DbType();
        }

        /// <summary>
        /// Set the database type for the column
        /// </summary>
        /// <param name="type">DB type to set</param>
        /// <returns>Self for chaining</returns>
        public Column DbType(DbType? type)
        {
            _field.DbType(type);

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
        public Column GetFormatter(Func<object, Dictionary<string, object>, object> fn)
        {
            _field.GetFormatter(fn);

            return this;
        }

        /// <summary>
        /// Get the 'Get' value for the field
        /// </summary>
        /// <returns>Get value</returns>
        public dynamic GetValue()
        {
            return _field.GetValue();
        }

        /// <summary>
        /// Set a "Get" value. When set this value is used to send to the
        /// client-side, regardless of what value is held by the database.
        ///  (if this field even has a database value!)
        /// </summary>
        /// <param name="val">Value to set for "Get"</param>
        /// <returns>Self for chaining</returns>
        public Column GetValue(object val)
        {
            _field.GetValue(val);

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
        public Column GetValue(Func<object> val)
        {
            _field.GetValue(val);

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
            return _field.Name();
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
        public Column Name(string name)
        {
            _field.Name(name);

            return this;
        }

        /// <summary>
        /// Get the Options object configured for this field
        /// </summary>
        /// <returns>Options object</returns>
        public Options Options()
        {
            return _field.Options();
        }

        /// <summary>
        /// Set a function that will retrieve a list of values that can be used
        /// for the options list in radio, select and checkbox inputs from the
        /// database for this field.
        /// </summary>
        /// <param name="fn">Delegate that will return a list of options</param>
        /// <returns>Self for chaining</returns>
        public Column Options(OptionsFunc fn)
        {
            _field.Options(fn);

            return this;
        }

        /// <summary>
        /// Set the options for this field using an Options instance
        /// </summary>
        /// <param name="opts">Configured options object</param>
        /// <returns>Self for chaining</returns>
        public Column Options(Options opts)
        {
            _field.Options(opts);

            return this;
        }

        /// <summary>
        /// Get the SearchBuilderOptions object configured for this field
        /// </summary>
        /// <returns>SearchBuilderOptions object</returns>
        public SearchBuilderOptions SearchBuilderOptions()
        {
            return _field.SearchBuilderOptions();
        }

        /// <summary>
        /// Set a function that will retrieve a list of values that can be used
        /// for the SearchBuilderOptions list in SearchBuilders for this field.
        /// </summary>
        /// <param name="fn">Delegate that will return a list of SearchBuilder options</param>
        /// <returns>Self for chaining</returns>
        public Column SearchBuilderOptions(
            Func<object, object, List<Dictionary<string, object>>> fn
        )
        {
            _field.SearchBuilderOptions(fn);

            return this;
        }

        /// <summary>
        /// Set the SearchBuilderOptions for this field using a SearchBuilderOptions instance
        /// </summary>
        /// <param name="opts">Configured SearchBuilderOptions object</param>
        /// <returns>Self for chaining</returns>
        public Column SearchBuilderOptions(SearchBuilderOptions sbOpts)
        {
            _field.SearchBuilderOptions(sbOpts);

            return this;
        }

        /// <summary>
        /// Get the SearchPaneOptions object configured for this field
        /// </summary>
        /// <returns>SearchPaneOptions object</returns>
        public SearchPaneOptions SearchPaneOptions()
        {
            return _field.SearchPaneOptions();
        }

        /// <summary>
        /// Set a function that will retrieve a list of values that can be used
        /// for the SearchPaneOptions list in SearchPanes for this field.
        /// </summary>
        /// <param name="fn">Delegate that will return a list of SearchPane options</param>
        /// <returns>Self for chaining</returns>
        public Column SearchPaneOptions(Func<object, object, List<Dictionary<string, object>>> fn)
        {
            _field.SearchPaneOptions(fn);

            return this;
        }

        /// <summary>
        /// Set the SearchPaneOptions for this field using a SearchPaneOptions instance
        /// </summary>
        /// <param name="opts">Configured SearchPaneOptions object</param>
        /// <returns>Self for chaining</returns>
        public Column SearchPaneOptions(SearchPaneOptions spOpts)
        {
            _field.SearchPaneOptions(spOpts);

            return this;
        }

        /// <summary>
        /// Get the field instance associated with this column
        /// </summary>
        /// <returns>Field instance</returns>
        internal Field Field()
        {
            return _field;
        }
    }
}
