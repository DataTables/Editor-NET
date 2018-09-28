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

            // Bind values
            for (int i = 0, ien = _bindings.Count; i < ien; i++)
            {
                var binding = _bindings[i];

                param = cmd.CreateParameter();
                param.ParameterName = binding.Name;
                param.Value = binding.Value ?? DBNull.Value;
                param.DbType = System.Data.DbType.Object;

                // Editor's type system is very weak, but Postgres is strong.
                // Postgres requires that numeric looking data is actually numeric
                try {
                    var str = binding.Value.ToString();
                    
                    if ( str.IndexOf('-') > 0 ) {

                    }
                    else {
                        param.Value = Convert.ToInt32(binding.Value);
                    }
                }
                catch {}

                // And DateTime
                try {
                    param.Value = DateTime.Parse(binding.Value);
                }
                catch {}

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
        override protected DataTables.Result _Exec()
        {
            var dt = new System.Data.DataTable();

            using (var dr = _stmt.ExecuteReader())
            {
                dt.Load(dr);
            }

            return new Postgres.Result(_db, dt, this);
        }
    }
}
