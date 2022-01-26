using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DataTables.DatabaseUtil.Oracle
{
    /// <summary>
    /// Oracle Query class - this should be considered to be beta.
    /// </summary>
    public class Query : DataTables.Query
    {
        /// <summary>
        /// Initialise the Oracle session
        /// </summary>
        /// <param name="dbh"></param>
        public new static void Init(Database dbh)
        {
            var provider = DbProviderFactories.GetFactory(dbh.Adapter());

            // Use ISO8601 for dates and times
            var cmd = provider.CreateCommand();
            cmd.CommandText = "ALTER SESSION SET NLS_DATE_FORMAT='YYYY-MM-DD HH24:MI:SS'";
            cmd.Connection = dbh.Conn();
            cmd.ExecuteNonQuery();

            var cmd2 = provider.CreateCommand();
            cmd2.CommandText = "ALTER SESSION SET NLS_TIMESTAMP_FORMAT = 'YYYY-MM-DD HH24:MI:SS'";
            cmd2.Connection = dbh.Conn();
            cmd2.ExecuteNonQuery();
        }

        /// <summary>
        /// Create a query instance specifically for Oracle
        /// </summary>
        /// <param name="db">Host database</param>
        /// <param name="type">Query type</param>
        public Query(Database db, string type)
            : base(db, type)
        {
        }

        internal override string _bindChar => ":";

        internal override string _fieldQuote => "\"";

        internal override string[] _identifierLimiter => new[] { "\"", "\"" };

        /// <summary>
        /// Bind parameters to the SQL statement
        /// </summary>
        /// <param name="sql">SQL command</param>
        protected override void _Prepare(string sql)
        {
            var provider = DbProviderFactories.GetFactory(_db.Adapter());
            var cmd = provider.CreateCommand();
            
            // Oracle.DataAccess.Client (and managed) bind by position by default(!)
            // So we need to force it to bind by name if used
            var bindByName = cmd.GetType().GetProperty("BindByName");
            bindByName?.SetValue(cmd, true, null);
            
            // Need to reliably get the primary key value
            if (_type == "insert" && _pkey != null)
            {
                // Add a returning parameter statement into an output parameter
                sql += " RETURNING " + _ProtectIdentifiers(_pkey[0]) + " INTO :dtvalue";
            }

            cmd.CommandText = sql;
            cmd.Connection = _db.Conn();
            cmd.Transaction = _db.DbTransaction;

            if (_db.CommandTimeout != -1) {
                cmd.CommandTimeout = _db.CommandTimeout;
            }

            // Need to reliably get the primary key value
            if (_type == "insert" && _pkey != null)
            {
                // Determine the parameter type
                var pkeyCmd = provider.CreateCommand();
                pkeyCmd.CommandText = "SELECT data_type, data_length FROM all_tab_columns WHERE table_name = upper(:t) AND column_name = upper(:c)";
                pkeyCmd.Connection = _db.Conn();
                pkeyCmd.Transaction = _db.DbTransaction;

                var pkeyByName = pkeyCmd.GetType().GetProperty("BindByName");
                pkeyByName?.SetValue(pkeyCmd, true, null);

                var tableParam = pkeyCmd.CreateParameter();
                tableParam.ParameterName = ":t";
                tableParam.Value = _table[0];
                pkeyCmd.Parameters.Add(tableParam);

                var pkeyParam = pkeyCmd.CreateParameter();
                pkeyParam.ParameterName = ":c";
                pkeyParam.Value = _pkey[0];
                pkeyCmd.Parameters.Add(pkeyParam);
                
                using (var dr = pkeyCmd.ExecuteReader())
                {
                    // If the table doesn't have a primary key field, we can't get
                    // the inserted pkey!
                    if (dr.HasRows && dr.Read())
                    {
                        var dataType = dr["data_type"];
                        var dataLength = dr["data_length"];

                        var outParam = cmd.CreateParameter();
                        outParam.ParameterName = ":dtvalue";
                        outParam.Direction = ParameterDirection.Output;

                        if ((string)dataType == "NUMBER")
                        {
                            outParam.DbType = DbType.Int32;
                        }
                        else if ((string)dataType == "DATE")
                        {
                            outParam.DbType = DbType.Date;
                        }
                        else if ((string)dataType == "DATETIME")
                        {
                            outParam.DbType = DbType.DateTime;
                        }
                        else
                        {
                            outParam.DbType = DbType.String;
                        }

                        cmd.Parameters.Add(outParam);
                        cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;
                    }
                    else
                    {
                        // Best effort
                        var outParam = cmd.CreateParameter();
                        outParam.ParameterName = ":dtvalue";
                        outParam.Direction = ParameterDirection.Output;
                        outParam.DbType = DbType.Int32;
                        cmd.Parameters.Add(outParam);
                        cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;
                    }
                }
            }

            // Bind values
            for (int i = 0, ien = _bindings.Count; i < ien; i++)
            {
                var binding = _bindings[i];

                var param = cmd.CreateParameter();
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
            var dt = new DataTable();

            /*
            if (_type == "insert")
            {
                _stmt.ExecuteNonQuery();
                return new Result(_db, null, this);
            }
            */

            using (var dr = _stmt.ExecuteReader())
            {
                dt.Load(dr);
            }
            
            return new Result(_db, dt, this);
        }
        
        /// <summary>	
        /// Oracle table statement	
        /// </summary>	
        /// <returns>SQL for the table</returns>	
        protected override string _BuildTable()	
        {	
            var tablesOut = new List<string>();	

             foreach (var t in _table)	
            {	
                if (t.Contains(" as "))	
                {	
                    var a = t.Split(new[] { " as " }, StringSplitOptions.None);	
                    tablesOut.Add(a[0] + " " + a[1]);	
                }	
                else	
                {	
                    tablesOut.Add(t);	
                }	
            }	

             return " " + string.Join(", ", tablesOut.ToArray()) + " ";	
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
                    limit += " OFFSET 0 ROWS";
                }

                limit += " FETCH NEXT " + _limit + " ROWS ONLY";
            }

            return limit;
        }
    }
}
