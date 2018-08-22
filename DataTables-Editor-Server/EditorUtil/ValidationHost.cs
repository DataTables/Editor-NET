// <copyright>Copyright (c) SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Information passed to validation methods about their host callers
// </summary>

namespace DataTables.EditorUtil
{
    /// <summary>
    /// When a field's value is validated, the validation method is given
    /// information about the host environment (i.e. the host Editor and Field)
    /// so it can perform database validation (for example the `Unique`
    /// validation)
    /// </summary>
    public class ValidationHost
    {
        /// <summary>
        /// Client-side requested action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Row Id being edited (for "edit" action)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Host field
        /// </summary>
        public Field Field { get; set; }

        /// <summary>
        /// Host Editor
        /// </summary>
        public Editor Editor { get; set; }

        /// <summary>
        /// Database connection instance
        /// </summary>
        public Database Db { get; set; }
    }
}
