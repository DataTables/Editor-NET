using System.Collections.Generic;
namespace DataTables.EditorUtil
{
    /// <summary>
    /// Container class to hold information about join details
    /// </summary>
    public class SearchPanesReturn
    {
        /// <summary>
        /// Join table name
        /// </summary>
        public Dictionary<string, object> options { get; set; }

        /// <summary>
        /// Left join information container
        /// </summary>
        /// <param name="table">Join table name</param>
        /// <param name="field1">Table 1 field</param>
        /// <param name="op">Join logic operator</param>
        /// <param name="field2">Table 2 field</param>
        internal SearchPanesReturn()
        {
            options = new Dictionary<string, object>();
        }
    }
}
