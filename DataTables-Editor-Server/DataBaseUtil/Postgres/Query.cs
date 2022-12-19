using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using DataTables.DatabaseUtil;

namespace DataTables.DatabaseUtil.Postgres
{
    /// <summary>
    /// Postgres Query class
    /// </summary>
    public class Query : DataTables.Query
    {
        /// <summary>
        /// Create a query instance specifically for Postgres
        /// </summary>
        /// <param name="db">Host database</param>
        /// <param name="type">Query type</param>
        public Query(Database db, string type)
            : base(db, type)
        {
        }

        override internal string _fieldQuote { get { return "\""; } }

        internal override string[] _identifierLimiter => new[] { "\"", "\"" };


        /// <summary>
        /// Bind parameters to the SQL statement
        /// </summary>
        /// <param name="sql">SQL command</param>
        override protected void _Prepare(string sql)
        {
            DbProviderFactory provider = DbProviderFactories.GetFactory(_db.Adapter());
            DbParameter param;
            DbCommand cmd = provider.CreateCommand();

            // Add a RETURNING command to postgres insert queries so we can get the
            // pkey value from the query reliably
            if (_type == "insert")
            {
                var table = _table[0].Split(new string[] { " as " }, StringSplitOptions.None);
                var pkeyCmd = provider.CreateCommand();

                pkeyCmd.CommandText =
                    @"SELECT 
					    pg_attribute.attname, 
					    format_type(pg_attribute.atttypid, pg_attribute.atttypmod) 
				    FROM pg_index, pg_class, pg_attribute 
				    WHERE 
					    pg_class.oid = @table::regclass AND
					    indrelid = pg_class.oid AND
					    pg_attribute.attrelid = pg_class.oid AND 
					    pg_attribute.attnum = any(pg_index.indkey)
					    AND indisprimary";
                pkeyCmd.Connection = _db.Conn();
                pkeyCmd.Transaction = _db.DbTransaction;

                param = pkeyCmd.CreateParameter();
                param.ParameterName = "@table";
                param.Value = table[0];
                pkeyCmd.Parameters.Add(param);

                using (var dr = pkeyCmd.ExecuteReader())
                {
                    // If the table doesn't have a primary key field, we can't get
                    // the inserted pkey!
                    if (dr.HasRows && dr.Read())
                    {
                        sql += " RETURNING " + dr["attname"] + " as dt_pkey";
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
                param.DbType = System.Data.DbType.Object;

                if (binding.Value == null) {
                    param.Value = DBNull.Value;
                }
                else if (binding.Type == null) {
                    // No binding type specified, attempt to create the correct type for Postgres.
                    // Editor's type system is very weak, but Postgres is strong.
                    // Postgres requires that numeric looking data is actually numeric
                    Type t = binding.Value.GetType();

                    // Transform based on the model properties
                    if (t.Name == "Decimal") {
                        param.Value = Convert.ToDecimal(binding.Value);
                        param.DbType = System.Data.DbType.Decimal;
                    }
                    else if (t.Name == "Int32") {
                        param.Value = Convert.ToInt32(binding.Value);
                        param.DbType = System.Data.DbType.Int32;
                    }
                    else {
                        // Really simple numbers should be treated as integers
                        try {
                            var str = binding.Value.ToString();

                            if ( IsDigitsOnly(str) ) {
                                param.Value = Convert.ToInt32(binding.Value);
                                param.DbType = System.Data.DbType.Int32;
                            }
                        }
                        catch {}

                        // Attempt to auto detect date and time values by parsing the data
                        try {
                            var str = binding.Value.ToString();

                            if (str.IndexOf(',') >= 0 || (str.IndexOf('/') == -1 && str.IndexOf('-') == -1)) {
                                // noop
                            }
                            else {
                                param.Value = DateTime.Parse(binding.Value);
                                param.DbType = System.Data.DbType.DateTime;
                            }
                        }
                        catch {}
                    }
                }
                else {
                    param.DbType = binding.Type;

                    if (
                        binding.Type == System.Data.DbType.Date ||
                        binding.Type == System.Data.DbType.DateTime ||
                        binding.Type == System.Data.DbType.DateTime2
                    ) {
                        param.Value = DateTime.Parse(binding.Value);
                    }
                    else if (binding.Type == System.Data.DbType.Int16)
                    {
                        param.Value = Convert.ToInt16(binding.Value);
                    }
                    else if (binding.Type == System.Data.DbType.Int32)
                    {
                        param.Value = Convert.ToInt32(binding.Value);
                    }
                    else if (binding.Type == System.Data.DbType.Int64)
                    {
                        param.Value = Convert.ToInt64(binding.Value);
                    }
                    else if (binding.Type == System.Data.DbType.Decimal)
                    {
                        param.Value = Convert.ToDecimal(binding.Value);
                    }
                    else if (binding.Type == System.Data.DbType.Double)
                    {
                        param.Value = Convert.ToDouble(binding.Value);
                    }
                    else if (binding.Type == System.Data.DbType.AnsiString)
                    {
                        param.Value = binding.Value.ToString();
                    }
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
        override protected DataTables.Result _Exec()
        {
            var dt = new System.Data.DataTable();

            using (var dr = _stmt.ExecuteReader())
            {
                dt.Load(dr);
            }

            return new Postgres.Result(_db, dt, this);
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
