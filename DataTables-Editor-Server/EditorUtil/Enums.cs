using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                    .Cast<object>()
                    .ToDictionary(e => Convert.ChangeType(e, underlyingType).ToString(), e => GetEnumDescription((Enum)e));
            }

            return Enum.GetValues(typeof(T))
                .Cast<object>()
                .ToDictionary(e => e.ToString(), e => GetEnumDescription((Enum)e));
        }
        private static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return value.ToString();

            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }
    }
}