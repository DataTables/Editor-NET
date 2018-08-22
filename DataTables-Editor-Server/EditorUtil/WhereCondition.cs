using System;

namespace DataTables.EditorUtil
{
    internal class WhereCondition
    {
        public string Key { get; set; }
        public dynamic Value { get; set; }
        public string Operator { get; set; }
        public Action<Query> Custom { get; set; }
    }
}
