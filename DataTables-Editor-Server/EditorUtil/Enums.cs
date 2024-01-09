using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTables.EditorUtil
{
    internal class Enums
    {
        internal static Dictionary<string, string> ConvertToStringDictionary<T>(bool useValueAsKey)
        {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException(typeof(T).Name + " must be an enum type.");
            }

            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            if (useValueAsKey) {
                return Enum.GetValues(typeof(T))
                    .Cast<T>()
                    .ToDictionary(e => Convert.ChangeType(e, underlyingType).ToString(), e => e.ToString());
            }

            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(e => e.ToString(), e => e.ToString());
        }
    }
}