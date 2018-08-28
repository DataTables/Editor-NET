using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTables.EditorUtil
{
    internal class NestedData
    {
        /// <summary>
        /// Check is a parameter is in the submitted data set. This is functionally
        /// the same as the `_readProp()` method, but in this case a binary value
        /// is required to indicate if the value is present or not.
        /// </summary>
        /// <param name="name">Javascript dotted object name to write to</param>
        /// <param name="data">Data source array to read from</param>
        /// <returns>`true` if present, `false` otherwise</returns>
        internal static bool InData(string name, Dictionary<string, object> data)
        {
            if (!name.Contains('.'))
            {
                return data.ContainsKey(name);
            }

            var names = name.Split('.');
            var inner = data;

            for (int i = 0, ien = names.Length - 1; i < ien; i++)
            {
                if (!inner.ContainsKey(names[i]))
                {
                    return false;
                }

                inner = inner[names[i]] as Dictionary<string, object>;
            }

            return inner.ContainsKey(names.Last());
        }


        /// <summary>
        /// Read a value from a data structure, using Javascript dotted object
        /// notation. This is the inverse of the `_writeProp` method and provides
        /// the same support, matching DataTables' ability to read nested JSON
        /// data objects.
        /// </summary>
        /// <param name="name">Javascript dotted object name to write to</param>
        /// <param name="inData">Data source array to read from</param>
        /// <returns>The read value, or null if no value found.</returns>
        internal static object ReadProp(string name, IDictionary<string, object> inData)
        {
            if (!name.Contains("."))
            {
                return inData.ContainsKey(name) ?
                    inData[name] :
                    null;
            }

            var names = name.Split('.');
            var inner = inData;

            for (var i = 0; i < names.Length - 1; i++)
            {
                if (!inner.ContainsKey(names[i]))
                {
                    return null;
                }

                inner = inner[names[i]] as IDictionary<string, object>;
            }

            return inner.ContainsKey(names.Last()) ?
                inner[names.Last()] :
                null;
        }

        /// <summary>
        /// Write the field's value to an array structure, using Javascript dotted
        /// object notation to indicate JSON data structure. For example `name.first`
        /// gives the data structure: `name: { first: ... }`. This matches DataTables
        /// own ability to do this on the client-side, although this doesn't
        /// implement implement quite such a complex structure (no array / function
        /// support).
        /// </summary>
        /// <param name="outData">Dic to write the data to</param>
        /// <param name="name">Javascript dotted object name to write to</param>
        /// <param name="value">Value to write</param>
        /// <param name="type">Type to convert to</param>
        internal static void WriteProp(Dictionary<string, object> outData, string name, object value, Type type)
        {
            if (!name.Contains("."))
            {
                WriteCast(outData, name, value, type);
                return;
            }

            var names = name.Split('.');
            var inner = outData;

            for (var i = 0; i < names.Length - 1; i++)
            {
                var loopName = names[i];

                if (!inner.ContainsKey(loopName))
                {
                    inner.Add(loopName, new Dictionary<string, object>());
                }
                else if (!(inner[loopName] is Dictionary<string, object>))
                {
                    throw new Exception(
                        "A property with the name `" + name + "` already exists. This " +
                        "can occur if you have properties which share a prefix - " +
                        "for example `name` and `name.first`"
                    );
                }

                inner = inner[loopName] as Dictionary<string, object>;
            }

            if (inner.ContainsKey(names.Last()))
            {
                throw new Exception(
                    "Duplicate field detected - a field with the name `" + name + "` " +
                    "already exists."
                );
            }

            // Attempt to cast to the type given
            WriteCast(inner, names.Last(), value, type);
        }


        /// <summary>
        /// Convert the value to the field's type, with error handling
        /// </summary>
        /// <param name="outData">Dic to write the data to</param>
        /// <param name="name">Javascript dotted object name to write to</param>
        /// <param name="value">Value to write</param>
        /// <param name="type">Type to convert to</param>
        internal static void WriteCast(Dictionary<string, object> outData, string name, object value, Type type)
        {
            try
            {
                outData.Add(name, value == null || value == System.DBNull.Value
                    ? null
                    : Convert.ChangeType(value, type)
                );
            }
            catch (Exception)
            {
                throw new Exception("Unable to cast value to be " + type.Name);
            }
        }
    }
}
