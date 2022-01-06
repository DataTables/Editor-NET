using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using IFormFile = Microsoft.AspNetCore.Http.IFormFile;
#else
using System.Web;
using IFormFile = System.Web.HttpPostedFile;
#endif


namespace DataTables
{
    /// <summary>
    /// Upload class for Editor. This class provides the ability to easily specify
    /// file upload information, specifically how the file should be recorded on
    /// the server (database and file system).
    /// 
    /// An instance of this class is attached to a field using the 'Field.upload()'
    /// method. When Editor detects a file upload for that file the information
    /// provided for this instance is executed.
    /// 
    /// The configuration is primarily driven through the 'db' and 'action' methods
    /// </summary>
    public class Upload
    {
        /*  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *
         * Public parameters
         */

        /// <summary>
        /// Database upload options for the 'fields' option in the 'Db()' method.
        /// These are used to provide easy information about the file that will be
        /// stored in the database.
        /// </summary>
        public enum DbType
        {
            /// <summary>
            /// Binary information - stored in a string column type
            /// </summary>
            Content,

            /// <summary>
            /// Binary information - stored in a binary column type
            /// </summary>
            ContentBinary,

            /// <summary>
            /// Content type
            /// </summary>
            ContentType,

            /// <summary>
            /// File extension (note that this includes the dot)
            /// </summary>
            Extn,

            /// <summary>
            /// File name (with extension)
            /// </summary>
            FileName,

            /// <summary>
            /// File size (bytes)
            /// </summary>
            FileSize,

            /// <summary>
            /// MIME type (same as content type)
            /// </summary>
            MimeType,

            /// <summary>
            /// HTTP path to the file this is computed from
            /// Request.PhysicalApplicationPath . If you are storing the files outside
            /// of your application, this option isn't particularly useful!
            /// </summary>
            WebPath,

            /// <summary>
            /// System path to the file (i.e. the absolute path on your hard disk)
            /// </summary>
            SystemPath,

            /// <summary>
            /// Don't write to the database, just read (default value or updated
            /// else where)
            /// </summary>
            ReadOnly
        }


        /*  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *
         * Private parameters
         */
        private string _actionStr;
        private IEnumerable<string> _extns;
        private string _extnError;
        private string _dbTable;
        private string _dbPKey;
        private Dictionary<string, object> _dbFields;
        private string _error;
        private Func<List<Dictionary<string, object>>, bool> _dbCleanCallback;
        private string _dbCleanTableField;
        private readonly List<Action<Query>> _where = new List<Action<Query>>();
        private Func<IFormFile, dynamic, dynamic> _actionFn;
        private readonly List<Func<IFormFile, string>> _validators = new List<Func<IFormFile, string>>();

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructor
         */

        /// <summary>
        /// Upload constructor
        /// </summary>
        public Upload()
        { }

        /// <summary>
        /// Upload constructor with a path action
        /// </summary>
        /// <param name="action">Location for where to store the file. This should be
        /// an absolute path on your system.</param>
        public Upload(string action)
        {
            Action(action);
        }

        /// <summary>
        /// Upload constructor with a function action
        /// </summary>
        /// <param name="action">Callback function that is executed when a file
        /// is uploaded.</param>
        public Upload(Func<IFormFile, dynamic, dynamic> action)
        {
            Action(action);
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public methods
         */

        /// <summary>
        /// Set the action to take when a file is uploaded. As a string the value
        /// given is the full system path to where the uploaded file is written to.
        /// The value given can include three "macros" which are replaced by the
        /// script dependent on the uploaded file:
        /// 
        /// * '__EXTN__' - the file extension (with the dot)
        /// * '__NAME__' - the uploaded file's name (including the extension)
        /// * '__ID__' - Database primary key value if the 'Db()' method is used
        /// </summary>
        /// <param name="action">Full system path for where the file should be stored</param>
        /// <returns>Self for chaining</returns>
        public Upload Action(string action)
        {
            _actionStr = action;
            _actionFn = null;

            return this;
        }

        /// <summary>
        /// Set the action to take when a file is uploaded. As a function the callback
        /// is given the responsiblity of what to do with the uploaded file. That will
        /// typically involve writing it to the file system so it can be used later.
        /// </summary>
        /// <param name="action">Callback</param>
        /// <returns>Self for chaining</returns>
        public Upload Action(Func<IFormFile, dynamic, dynamic> action)
        {
            _actionStr = null;
            _actionFn = action;

            return this;
        }

        /// <summary>
        /// A list of valid file extensions that can be uploaded. This is for simple
        /// validation that the file is of the expected type. The check is
        /// case-insensitive. If no extensions are given, no validation is performed
        /// on the file extension.
        /// </summary>
        /// <param name="extns">List of extensions to test against.</param>
        /// <param name="error">Error message for if the file is not valid.</param>
        /// <returns>Self for chaining</returns>
        /// <deprecated>Use the Validation.FileExtensions() method instead</deprecated>
        public Upload AllowedExtensions(IEnumerable<string> extns, string error = "This file type cannot be uploaded")
        {
            _extns = extns;
            _extnError = error;

            return this;
        }

        /// <summary>
        /// Database configuration method. When used, this method will tell Editor
        /// what information you want to be wirtten to a database on file upload, should
        /// you wish to store relational information about your files on the database
        /// (this is generally recommended).
        /// </summary>
        /// <param name="table">Name of the table where the file information should be stored</param>
        /// <param name="pkey">Primary key column name. This is required so each row can
        /// be uniquely identified.</param>
        /// <param name="fields">A list of the fields to be wirtten to on upload. The
        /// dictonary keys are used as the database column names and the values can be
        /// defined by the 'DbType' enum of this class. The value can also be a string,
        /// which will be written directly to the database, or a function which will be
        /// executed and the returned value written to the database.</param>
        /// <returns>Self for chanining</returns>
        public Upload Db(string table, string pkey, Dictionary<string, object> fields)
        {
            _dbTable = table;
            _dbPKey = pkey;
            _dbFields = fields;

            return this;
        }

        /// <summary>
        /// Set a callback function that is used to remove files which no longer have
        /// a reference in a source table.
        /// </summary>
        /// <param name="callback">
        /// Function that will be executed on clean. It is given a List of information
        /// from the database about the orphaned rows, and can return true to indicate
        /// that the rows should be removed from the database. Any other return value
        /// (including none) will result in the records being retained.
        /// </param>
        /// <returns>Self for chaining</returns>
        public Upload DbClean(Func<List<Dictionary<string, object>>, bool> callback)
        {
            _dbCleanCallback = callback;

            return this;
        }

        /// <summary>
        /// Set a callback function that is used to remove files which no longer have
        /// a reference in a source table.
        /// </summary>
        /// <param name="tableField">
        /// The table and field ("table.field" format) that should be used to check and
        /// see if a file reference is being used.
        /// </param>
        /// <param name="callback">
        /// Function that will be executed on clean. It is given a List of information
        /// from the database about the orphaned rows, and can return true to indicate
        /// that the rows should be removed from the database. Any other return value
        /// (including none) will result in the records being retained.
        /// </param>
        /// <returns>Self for chaining</returns>
        public Upload DbClean(string tableField, Func<List<Dictionary<string, object>>, bool> callback)
        {
            _dbCleanCallback = callback;
            _dbCleanTableField = tableField;

            return this;
        }

        /// <summary>
        /// Add a validation method to check file uploads. Multiple validators can be
        /// added by calling this method multiple times. They will be executed in
        /// sequence when a file has been uploaded.
        /// </summary>
        /// <param name="fn">Validation function. The function takes a single parameter,
        /// an HttpPostedFile, and a string is returned on error with the error message.
        /// If the validation does not fail, 'null' should be returned.</param>
        /// <returns>Self for chaining</returns>
        public Upload Validator(Func<IFormFile, string> fn)
        {
            _validators.Add(fn);

            return this;
        }

        /// <summary>
        /// Add one or more WHERE conditions to the data that is retrieved from the database
        /// when querying it for the list of available files in a table.
        /// </summary>
        /// <param name="fn">Delegate to execute adding where conditions to the query</param>
        /// <returns>Self for chaining</returns>
        public Upload Where(Action<Query> fn)
        {
            _where.Add(fn);

            return this;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Internal methods
         */

        /// <summary>
        /// Get database information from the table
        /// </summary>
        /// <param name="db">Database instance</param>
        /// <param name="ids">Limit the results to a collection of ids</param>
        /// <returns>Database information</returns>
        internal Dictionary<string, Dictionary<string, object>> Data(Database db, ICollection<object> ids = null)
        {
            if (_dbTable == null)
            {
                return null;
            }

            // Select the configured db columns
            var q = db.Query("select")
                .Table(_dbTable)
                .Get(_dbPKey);

            foreach (var pair in _dbFields)
            {
                var column = pair.Key;
                var prop = pair.Value;

                if (prop is DbType)
                {
                    if ((DbType)prop != DbType.Content && (DbType)prop != DbType.ContentBinary)
                    {
                        q.Get(column);
                    }
                }
                else
                {
                    q.Get(column);
                }
            }

            if (ids != null)
            {
                q.WhereIn(_dbPKey, ids);
            }

            foreach (var condition in _where)
            {
                q.Where(condition);
            }

            var result = q.Exec();
            var outData = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> row;

            while ((row = result.Fetch()) != null)
            {
                outData.Add(row[_dbPKey].ToString(), row);
            }

            return outData;
        }


        /// <summary>
        /// Execute a file clean up
        /// </summary>
        /// <param name="editor">Calling Editor instance</param>
        /// <param name="field">Host field</param>
        internal void DbCleanExec(Editor editor, Field field)
        {
            var tables = editor.Table();
            _DbClean(editor.Db(), tables.First(), field.DbField());
        }


        /// <summary>
        /// Get the error message for the uplaod
        /// </summary>
        /// <returns>Error message</returns>
        internal string Error()
        {
            return _error;
        }

        /// <summary>
        /// Execute an upload
        /// </summary>
        /// <param name="editor">Host editor</param>
        /// <returns>Id of the new file</returns>
        internal dynamic Exec(Editor editor)
        {
            dynamic id = null;
            var files = editor.RequestFiles();
            var upload = files["upload"];

            // Validation of input files
            if (upload == null)
            {
                _error = "No file uploaded";
                return false;
            }

            // NOTE handling errors where the file size uploaded is larger than
            // that allowed must be handled in Global.aspx.cs
            // http://stackoverflow.com/questions/2759193

            // Validation - acceptable files extensions
            if (_extns != null && _extns.Any())
            {
                var extension = Path.GetExtension(upload.FileName).ToLower();

                if (_extns.Select(x => x.ToLower()).ToList().Contains(extension) == false)
                {
                    _error = _extnError;
                    return false;
                }
            }

            // Validation - custom callbacks
            foreach (var validator in _validators)
            {
                var res = validator(upload);

                if (res != null)
                {
                    _error = res;
                    return false;
                }
            }

            if (_dbTable != null)
            {
                foreach (var pair in _dbFields)
                {
                    // We can't know what the path is, if it has moved into place
                    // by an external function - throw an error if this does happen
                    var column = pair.Key;
                    var prop = pair.Value;

                    if (_actionStr == null && prop is DbType &&
                        ((DbType)prop == DbType.SystemPath || (DbType)prop == DbType.WebPath))
                    {
                        _error = "Cannot set path information in database " +
                            "if a custom method is used to save the file.";
                        return false;
                    }
                }

                // Commit to the database
                id = _dbExec(editor, upload);
            }

            // Perform file system actions
            return _actionExec(id, upload);
        }

        /// <summary>
        /// Get the primary key of the files table
        /// </summary>
        /// <returns>Primary key column</returns>
        internal string Pkey()
        {
            return _dbPKey;
        }

        /// <summary>
        /// Get the table name for the files table
        /// </summary>
        /// <returns>Table name</returns>
        internal string Table()
        {
            return _dbTable;
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private methods
         */

        /// <summary>
        /// Execute the configured action for the upload
        /// </summary>
        /// <param name="id">Primary key value</param>
        /// <param name="upload">Posted file</param>
        /// <returns>File identifier - typically the primary key</returns>
        private dynamic _actionExec(dynamic id, IFormFile upload)
        {
            if (_actionStr == null)
            {
                // Custom function
                return _actionFn != null ?
                    _actionFn(upload, id) :
                    id;
            }

            // Default action - move the file to the location specified by the
            // action string
            string to = _path(_actionStr, upload.FileName, id);

            try
            {
#if NETCOREAPP
                using (var readStream = upload.OpenReadStream())
                {
                    using (var writeStream = new StreamWriter(to))
                    {
                        readStream.CopyTo(writeStream.BaseStream);
                    }
                }
#else
                upload.SaveAs(to);
#endif
            }
            catch (Exception e)
            {
                _error = "Error saving file. " + e.Message;
                return false;
            }

            return id ?? to;
        }

        private void _DbClean(Database db, string editorTable, string fieldName)
        {
            if (_dbTable == null || _dbCleanCallback == null)
            {
                return;
            }

            if (_dbCleanTableField != null)
            {
                fieldName = _dbCleanTableField;
            }

            string table;
            string field;
            var a = fieldName.Split(new[] { '.' });
            if (a.Length == 1)
            {
                table = editorTable;
                field = a[0];
            }
            else
            {
                // editorTable can be Table or schema.Table
                // fieldName can be Field (a.lenght == 1), Table.Field or schema.Table.Field
                var editorTableName = editorTable.Split(new[] { '.' }).Last();
                var fieldTableName = a.Length == 2 ? a[0] : a[1];
                table = fieldTableName == editorTableName ? editorTable : string.Join(".", a.Take(a.Length - 1));
                field = a.Last();
            }

            // Get the infromation from the database about the orphaned children
            var q = db.Query("select")
                .Table(_dbTable)
                .Get(_dbPKey);

            foreach (var pair in _dbFields)
            {
                var column = pair.Key;
                var prop = pair.Value;

                if (prop is DbType && (DbType)prop != DbType.Content)
                {
                    q.Get(column);
                }
            }

            q.Where(_dbPKey, "(SELECT " + field + " FROM " + table + " WHERE " + field + " IS NOT NULL)", "NOT IN", false);

            var data = q.Exec().FetchAll();

            if (!data.Any())
            {
                return;
            }

            // Delete the selected rows, iff the develoeprs says to do so with a
            // true return.
            if (_dbCleanCallback(data) != true)
            {
                return;
            }

            var qDelete = db.Query("delete")
                .Table(_dbTable);

            foreach (var row in data)
            {
                qDelete.OrWhere(_dbPKey, row[_dbPKey].ToString());
            }

            qDelete.Exec();
        }

        /// <summary>
        /// Add a record to the database for a newly uploaded file
        /// </summary>
        /// <param name="editor">Host editor</param>
        /// <param name="upload">Uploaded file</param>
        /// <returns>Primary key value for the newly uploaded file</returns>
        private dynamic _dbExec(Editor editor, IFormFile upload)
        {
            var db = editor.Db();
            var pathFields = new Dictionary<string, string>();

            // Insert the details requested, for the columns requested
            var q = db.Query("insert")
                .Table(_dbTable)
                .Pkey(new[] { _dbPKey });

            foreach (var pair in _dbFields)
            {
                var column = pair.Key;
                var prop = pair.Value;
#if NETCOREAPP
                var contentLength = (int)upload.Length;
#else
                var contentLength = upload.ContentLength;
#endif

                if (prop is DbType)
                {
                    var propType = (DbType)prop;

                    switch (propType)
                    {
                        case DbType.ReadOnly:
                            break;

                        case DbType.Content:
                            q.Set(column, upload.ToString());
                            break;

                        case DbType.ContentBinary:
                            byte[] fileData = null;
#if NETCOREAPP
                            var stream = upload.OpenReadStream();
#else
                            var stream = upload.InputStream;
#endif
                            using (var binaryReader = new BinaryReader(stream))
                            {
                                fileData = binaryReader.ReadBytes(contentLength);
                                q.Set(column, fileData);
                            }
                            break;

                        case DbType.ContentType:
                        case DbType.MimeType:
                            q.Set(column, upload.ContentType);
                            break;

                        case DbType.Extn:
                            q.Set(column, Path.GetExtension(upload.FileName));
                            break;

                        case DbType.FileName:
                            q.Set(column, Path.GetFileName(upload.FileName));
                            break;

                        case DbType.FileSize:
                            q.Set(column, contentLength);
                            break;

                        case DbType.SystemPath:
                            pathFields.Add(column, "__SYSTEM_PATH__");
                            q.Set(column, "-"); // Use a temporary value to avoid cases
                            break; // where the db will reject empty values

                        case DbType.WebPath:
                            pathFields.Add(column, "__WEB_PATH__");
                            q.Set(column, "-"); // Use a temporary value (as above)
                            break;

                        default:
                            throw new Exception("Unknown database type");
                    }
                }
                else
                {
                    dynamic val = prop;

                    try
                    {
                        // Callable function - execute to get the value
                        var propFn = (Func<Database, IFormFile, dynamic>)prop;
                        val = propFn(db, upload);
                    }
                    catch (Exception) {}

                    if (val is string) {
                        // Allow for replacement of __ID__, etc when the value is a string
                        pathFields.Add(column, val);
                        q.Set(column, "-"); // Use a temporary value (as above)
                    }
                    else {
                        q.Set(column, val);
                    }
                }
            }

            var res = q.Exec();
            var id = res.InsertId();

            // Update the newly inserted row with the path information. We have to
            // use a second statement here as we don't know in advance what the
            // database schema is and don't want to prescribe that certain triggers
            // etc be created. It makes it a bit less efficient but much more
            // compatible
            if (pathFields.Any() && _actionStr != null)
            {
                // For this to operate the action must be a string, which is
                // validated in the `exec` method
                var path = _path(_actionStr, upload.FileName, id);
#if NETCOREAPP
                var physicalPath = Directory.GetCurrentDirectory() ?? "";
                var webPath = physicalPath.Length != 0 ?
                    path.Replace(physicalPath, "") :
                    "";
#else
                var physicalPath = editor.Request().PhysicalApplicationPath ?? "";
                var webPath = physicalPath.Length != 0 ?
                    path.Replace(physicalPath, Path.DirectorySeparatorChar.ToString()) :
                    "";
#endif

                var pathQ = db
                    .Query("update")
                    .Table(_dbTable)
                    .Where(_dbPKey, id);

                foreach (var pathField in pathFields)
                {
                    var val = _path(pathField.Value, upload.FileName, id)
                        .Replace("__SYSTEM_PATH__", path)
                        .Replace("__WEB_PATH__", webPath);

                    pathQ.Set(pathField.Key, val);
                }

                pathQ.Exec();
            }

            return id;
        }

        /// <summary>
        /// Apply macros to a user specified path
        /// </summary>
        /// <param name="val">The value to be transformed</param>
        /// <param name="name">File path</param>
        /// <param name="id">Primary key value for the file</param>
        /// <returns>Resolved path</returns>
        private string _path(string val, string name, string id)
        {
            return val
                .Replace("__NAME__", Path.GetFileNameWithoutExtension(name))
                .Replace("__ID__", id)
                .Replace("__EXTN__", Path.GetExtension(name));
        }
    }
}
