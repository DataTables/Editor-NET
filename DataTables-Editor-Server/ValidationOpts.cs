// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Options to configure a validation method
// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// Common validation options that can be specified for all validation methods.
    /// </summary>
    public class ValidationOpts
    {
        private string _DependsField = null;
        private IEnumerable<object> _DependsValues = null;
        private Func<object, Dictionary<string, object>, ValidationHost, bool> _DependsFn = null;


        /// <summary>
        /// Error message should the validation fail
        /// </summary>
        public string Message = "Input not valid";

        /// <summary>
        /// Allow a field to be empty, i.e. a zero length string -
        /// `''` (`true` - default) or require it to be non-zero length (`false`).
        /// </summary>
        public bool? Empty = true;

        /// <summary>
        /// Require the field to be submitted (`false`) or not
        /// (`true` - default). When set to `true` the field does not need to be
        /// included in the list of parameters sent by the client - if set to `false`
        /// then it must be included. This option can be particularly useful in Editor
        /// as Editor will not set a value for fields which have not been submitted -
        /// giving the ability to submit just a partial list of options.
        /// </summary>
        public bool Optional = true;


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public static methods
         */

        /// <summary>
        /// Check to see if validation options have been given. If not, create
        /// and instance with the default options and return that. This is
        /// useful for checking to see if a user has passed in validation options
        /// or not, since a new instance can't be a default parameter value.
        /// </summary>
        /// <param name="user">User set validation options or null</param>
        /// <returns>Validation options to use for the validation</returns>
        public static ValidationOpts Select(ValidationOpts user)
        {
            return user ?? new ValidationOpts();
        }

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// If the given field has a value, this validator will be applied.
        /// </summary>
        /// <param name="field">Field to check for a value</param>
        /// <returns>Self for chaining</returns>
        public ValidationOpts DependsOn(string field)
        {
            _DependsField = field;

            return this;
        }

        /// <summary>
        /// If the given field has one of the value's specified, this validator will be applied.
        /// </summary>
        /// <param name="field">Field to check values for</param>
        /// <param name="values">Values to check against</param>
        /// <returns>Self for chaining</returns>
        public ValidationOpts DependsOn(string field, IEnumerable<string> values)
        {
            _DependsField = field;
            _DependsValues = values;

            return this;
        }

        /// <summary>
        /// Set a function that will be executed to see if a validator should be applied or not.
        /// </summary>
        /// <param name="fn">Callback function. The function should true if the validator should apply, false otherwise</param>
        /// <returns>Self for chaining</returns>
        public ValidationOpts DependsOn(Func<object, Dictionary<string, object>, ValidationHost, bool> fn)
        {
            _DependsFn = fn;

            return this;
        }

        /// <summary>
        /// Set if this field is allowed to be empty or not
        /// </summary>
        /// <param name="set">Indicate if the field is allowed to be empty</param>
        /// <returns>Self for chaining</returns>
        public ValidationOpts SetEmpty(bool set)
        {
            Empty = set;

            return this;
        }

        /// <summary>
        /// Set the error message for cases when the validator fails
        /// </summary>
        /// <param name="msg">Validation error message</param>
        /// <returns>Self for chaining</returns>
        public ValidationOpts SetMessage(string msg)
        {
            Message = msg;

            return this;
        }

        /// <summary>
        /// Set the `optional` flag for this validator
        /// </summary>
        /// <param name="set">Indicate if the field value is optional or not</param>
        /// <returns>Self for chaining</returns>
        public ValidationOpts SetOptional(bool set)
        {
            Optional = set;

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */

        /// <summary>
        /// Run the dependency check.
        /// </summary>
        /// <param name="val">Field value</param>
        /// <param name="data">Row data</param>
        /// <param name="host">Validation host</param>
        /// <returns>true if the validator should run, false otherwise</returns>
        internal bool RunDepends(object val, Dictionary<string, object> data, ValidationHost host)
        {
            if (_DependsFn != null)
            {
                // External function - call it
                return _DependsFn(val, data, host);
            }
            else if (_DependsField != null)
            {
                // Get the value that was submitted for the dependent field
                var depFieldVal = NestedData.ReadProp(_DependsField, data);

                if (_DependsValues != null)
                {
                    // Field and value
                    return _DependsValues.Contains(depFieldVal);
                }

                // Just a field - check that the field has a value
                return depFieldVal != null && (string)depFieldVal != "";
            }

            // Default is to apply the validator
            return true;
        }
    }
}
