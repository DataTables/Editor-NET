using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using DataTables.DatabaseUtil;

namespace DataTables.DatabaseUtil.Sqlserver
{
    /// <summary>
    /// SQL Server Query class
    /// </summary>
    public class Query : DataTables.Query
    {
        internal override string[] _identifierLimiter => new[] { "[", "]" };

        /// <summary>
        /// Create a query instance specifically for SQL Server
        /// </summary>
        /// <param name="db">Host database</param>
        /// <param name="type">Query type</param>
        public Query(Database db, string type)
            : base(db, type)
        {
        }

        /// <summary>
        /// Create LIMIT / OFFSET for SQL Server 2012+. Note that this will only work
        /// with SQL Server 2012 or newer due to the use of the OFFSET and FETCH NEXT
        /// keywords
        /// </summary>
        /// <returns>Limit / offset string</returns>
        override protected string _BuildLimit()
        {
            string limit = "";

            if (_offset != -1)
            {
                limit = " OFFSET " + _offset + " ROWS";
            }

            if (_limit != -1)
            {
                if (_offset == -1)
                {
                    // If you get an error here, it is probably because you haven't
                    // specified an order clause, which is required by SQL Server
                    // https://www.microsoftpressstore.com/articles/article.aspx?p=2314819
                    limit += " OFFSET 0 ROWS";
                }

                limit += " FETCH NEXT " + _limit + " ROWS ONLY";
            }

            return limit;
        }


        /// <summary>
        /// Bind parameters to the SQL statement
        /// </summary>
        /// <param name="sql">SQL command</param>
        override protected void _Prepare(string sql)
        {
            DbParameter param;
            var provider = DbProviderFactories.GetFactory(_db.Adapter());
            var cmd = provider.CreateCommand();

            // On insert we need to get the table's primary key value in
            // an 'output' statement, so it can be used
            if (_type == "insert")
            {
                var pkeyCmd = provider.CreateCommand();
                var parts = _table[0].Split(new [] {'.'});
                var schemaName = parts.Count() > 1 ? parts[0] : "";
                var tableName = parts.Count() > 1 ? parts[1] : _table[0];
                var pkey = Pkey();

                if (pkey != null && pkey.Count() == 1) {
                    // We've got a primary key name - we need to determine its data type
                    var schemaQuery = schemaName != "" ?
                        " TABLE_SCHEMA = @schema AND " :
                        "";

                    // Note that readin the column name is rather redundant here since we
                    // already know it - but it means the code below can be used for both
                    // the known and unknown state
                    pkeyCmd.CommandText = @"
                        SELECT
                            DATA_TYPE as data_type,
                            CHARACTER_MAXIMUM_LENGTH as data_length,
                            COLUMN_NAME as column_name
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE 
                            " + schemaQuery + @"
                            TABLE_NAME   = @table AND 
                            COLUMN_NAME  = @column
                    ";

                    var column = pkey[0];

                    if (column.Contains(".")) {
                        var split = column.Split(new [] {'.'});
                        column = split.Last();
                    }

                    param = pkeyCmd.CreateParameter();
                    param.ParameterName = "@column";
                    param.Value = column;
                    pkeyCmd.Parameters.Add(param);
                }
                else {
                    // Don't have the primary key name - need to try and work out what it is
                    var schemaQuery = schemaName != "" ?
                        " KCU.TABLE_SCHEMA = @schema AND " :
                        "";

                    // We need to find out what the primary key column name and type is
                    pkeyCmd.CommandText = @"
                        SELECT
                            KCU.table_name as table_name,
                            KCU.column_name as column_name,
                            C.DATA_TYPE as data_type,
                            C.CHARACTER_MAXIMUM_LENGTH as data_length
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU ON
                            TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND
                            TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME AND " +
                            schemaQuery +
                            @"KCU.TABLE_NAME = @table
                        JOIN
                            INFORMATION_SCHEMA.COLUMNS as C ON
                                C.table_name = KCU.table_name AND
                                C.column_name = KCU.column_name
                        ORDER BY KCU.TABLE_NAME, KCU.ORDINAL_POSITION
                    ";
                }

                pkeyCmd.Connection = _db.Conn();
                pkeyCmd.Transaction = _db.DbTransaction;

                param = pkeyCmd.CreateParameter();
                param.ParameterName = "@table";
                param.Value = tableName;
                pkeyCmd.Parameters.Add(param);

                if (schemaName != "") {
                    param = pkeyCmd.CreateParameter();
                    param.ParameterName = "@schema";
                    param.Value = schemaName;
                    pkeyCmd.Parameters.Add(param);
                }

                using (var dr = pkeyCmd.ExecuteReader())
                {
                    // If the table doesn't have a primary key field, we can't get
                    // the inserted pkey!
                    if (dr.HasRows && dr.Read())
                    {
                        // Insert into a temporary table so we can select from it.
                        // This is required for tables which have a trigger on insert
                        // See thread 29556. We can't just use 'SELECT SCOPE_IDENTITY()'
                        // since the primary key might not be an identity column
                        sql = dr["data_length"] != DBNull.Value ?
                            "DECLARE @T TABLE ( insert_id " + dr["data_type"] + " (" + dr["data_length"] + ") ); " + sql :
                            "DECLARE @T TABLE ( insert_id " + dr["data_type"] + " ); " + sql;
                        sql = sql.Replace(" VALUES (",
                            " OUTPUT INSERTED." + dr["column_name"] + " as insert_id INTO @T VALUES (");
                        sql += "; SELECT insert_id FROM @T";
                    }
                    else {
                        _db.DebugInfo("No pkey data found");
                    }
                }
            }

            cmd.CommandText = sql;
            cmd.Connection = _db.Conn();
            cmd.Transaction = _db.DbTransaction;

            if (_db.CommandTimeout != -1) {
                cmd.CommandTimeout = _db.CommandTimeout;
            }

            // Bind values
            for (int i = 0, ien = _bindings.Count; i < ien; i++)
            {
                var binding = _bindings[i];

                param = cmd.CreateParameter();
                param.ParameterName = binding.Name;
                param.Value = binding.Value ?? DBNull.Value;

                if (binding.Type != null)
                {
                    param.DbType = binding.Type;
                }

                cmd.Parameters.Add(param);
            }

            _stmt = cmd;

            _db.DebugInfo(sql, _bindings);
        }


        /// <summary>
        /// Execute the SQL command
        /// </summary>
        /// <returns>Result instance</returns>
        protected override DataTables.Result _Exec()
        {
            var dt = new System.Data.DataTable();

            using (var dr = _stmt.ExecuteReader())
            {
                dt.Load(dr);
            }

            return new Sqlserver.Result(_db, dt, this);
        }
    }
}
