using System.Collections.Generic;
using DataTables.EditorUtil;

namespace DataTables.EditorUtil
{
    /// <summary>
    /// SQL statement debug information
    /// </summary>
    public class DebugInfo
    {
        /// <summary>
        /// SQL query
        /// </summary>
        public string Query;

        /// <summary>
        /// Information about bound variables (if any)
        /// </summary>
        public List<Binding> Bindings;
    }
}
