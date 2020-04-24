﻿// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Attributes that can be used for properties in Editor models
// </summary>
using System.Collections.Generic;
using System.Linq;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// DataTables and Editor response object. This object can be used to
    /// construct and contain the data in response to a DataTables or Editor
    /// request before JSON encoding it and sending to the client-side.
    ///
    /// Note that this object uses lowercase property names as this it output
    /// directly to JSON, so the format and parameter names that DataTables and
    /// Editor expect must be used.
    /// </summary>
    public class DtResponse
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public properties
         */

        /* Server-side processing parameters */

        /// <summary>
        /// Draw counter for server-side processing requests
        /// </summary>
        public int? draw;

        /// <summary>
        /// Data to draw the table with, for both client-side and server-side processing
        /// </summary>
        public List<Dictionary<string, object>> data =
            new List<Dictionary<string, object>>();

        /// <summary>
        /// Total record count for server-side processing requests
        /// </summary>
        public int? recordsTotal;

        /// <summary>
        /// Record count in the filtered data set for server-side processing requests
        /// </summary>
        public int? recordsFiltered;


        /* Editor parameters */

        /// <summary>
        /// General error message if there is one
        /// </summary>
        public string error;

        /// <summary>
        /// List of field errors if one or more fields are in an error state
        /// when validated
        /// </summary>
        public List<FieldError> fieldErrors =
            new List<FieldError>();

        /// <summary>
        /// Id of the newly created row for the create action
        /// </summary>
        public int? id;

        /// <summary>
        /// Information that can be processes in the Ajax callback handlers can
        /// be added here. It is not actively used by the libraries.
        /// </summary>
        public Dictionary<string, object> meta =
            new Dictionary<string, object>();

        /// <summary>
        /// List of options for Editor `select`, `radio` and `checkbox` field types
        /// </summary>
        public Dictionary<string, object> options =
            new Dictionary<string, object>();

        /// <summary>
        /// Object containing a list of options from SearchPanes
        /// </summary>
        public SearchPanesReturn searchPanes = 
            new SearchPanesReturn();

        /// <summary>
        /// File information for the upload input types
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, object>>> files =
            new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

        /// <summary>
        /// Row data on update action
        /// </summary>
        public ResponseUpload upload =
            new ResponseUpload();

        /// <summary>
        /// If debug mode is enabled, this property will be populated with information
        /// about the SQL statements Editor has run.
        /// </summary>
        public List<object> debug;

        /// <summary>
        /// List of ids for row that have had their processing cancelled by the `pre*` events
        /// </summary>
        public List<object> cancelled  = new List<object>();



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Merge a response object into this one to create a single combined
        /// object. Generally parameters that are defined in the object passed
        /// in as a parameter will overwrite the parameters in this object if
        /// the are defined.
        /// </summary>
        /// <param name="b">Response object to merge in</param>
        /// <returns>Self for chaining</returns>
        public DtResponse Merge(DtResponse b)
        {
            if (b.draw != null)
            {
                this.draw = b.draw;
            }

            if (b.data.Count() != 0)
            {
                this.data = b.data;
            }

            if (b.error != null)
            {
                this.error = b.error;
            }

            if (b.fieldErrors != null)
            {
                this.fieldErrors = b.fieldErrors;
            }

            if (b.id != null)
            {
                this.id = b.id;
            }

            if (b.recordsTotal != null)
            {
                this.recordsTotal = b.recordsTotal;
            }

            if (b.recordsFiltered != null)
            {
                this.recordsFiltered = b.recordsFiltered;
            }

            if (b.options.Count() != 0)
            {
                this.options = b.options;
            }

            if (b.searchPanes.options.Count() != 0){
                this.searchPanes.options = b.searchPanes.options;
            }

            if (b.files.Count() != 0)
            {
                this.files = b.files;
            }

            return this;
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Nested class
         */

        /// <summary>
        /// Editor field error nested class. Describes an error message for a
        /// field if it is in an error state.
        /// </summary>
        public class FieldError
        {
            /// <summary>
            /// Name of the field in error state
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// Error message
            /// </summary>
            public string status { get; set; }
        }

        /// <summary>
        /// Upload response information
        /// </summary>
        public class ResponseUpload
        {
            /// <summary>
            /// Id of the newly uploaded file
            /// </summary>
            public dynamic id { get; set; }
        }
    }
}
