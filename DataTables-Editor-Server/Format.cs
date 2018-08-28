// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Formatter methods for Editor
// </summary>
using System;
using System.Collections.Generic;

namespace DataTables
{
    /// <summary>
    /// Formatter methods for the DataTables Editor. All of the methods in this
    /// class return a delegate that can be used in the <code>GetFormatter</code>
    /// and <code>SetFormatter</code> methods of the <code>Field</code> class.
    ///
    /// Each method may define its own parameters that configure how the
    /// formatter operates. For example the date / time formatters take information
    /// on the formatting to be used.
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// Date format: 2012-03-09. jQuery UI equivalent format: yy-mm-dd
        /// </summary>
        public const string DATE_ISO_8601 = "yyyy-MM-dd";

        /// <summary>
        /// Date format: Fri, 9 Mar 12. jQuery UI equivalent format: D, d M y
        /// </summary>
        public const string DATE_ISO_822 = "ddd, d MMM yy";

        /// <summary>
        /// Date format: Friday, 09-Mar-12.  jQuery UI equivalent format: DD, dd-M-y
        /// </summary>
        public const string DATE_ISO_850 = "dddd, dd-MMM-yy";

        /// <summary>
        /// Date format: Fri, 9 Mar 12. jQuery UI equivalent format: D, d M y
        /// </summary>
        public const string DATE_ISO_1036 = "ddd, d MMM yy";

        /// <summary>
        /// Date format: Fri, 9 Mar 2012. jQuery UI equivalent format: D, d M yy
        /// </summary>
        public const string DATE_ISO_1123 = "ddd, d MMM yyyy";

        /// <summary>
        /// Date format: Fri, 9 Mar 2012. jQuery UI equivalent format: D, d M yy
        /// </summary>
        public const string DATE_ISO_2822 = "ddd, d MMM yyyy";

        /// <summary>
        /// Date format: 03-09-2012 (US style)
        /// </summary>
        public const string DATE_USA = "MM-dd-yyyy";



        /// <summary>
        /// Convert from SQL date / date time format (ISO8601) to a format given by the options parameter.
        /// </summary>
        /// <param name="format">Value to convert from SQL date format</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> DateSqlToFormat(string format)
        {
            return DateTime("yyyy-MM-dd", format);
        }

        /// <summary>
        /// Convert to SQL date / date time format (ISO8601) from a format given by the options parameter.
        /// </summary>
        /// <param name="format">Value to convert to SQL date format</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> DateFormatToSql(string format)
        {
            return DateTime(format, "yyyy-MM-dd");
        }

        /// <summary>
        /// Convert from one date time format to another
        /// </summary>
        /// <param name="from">From format</param>
        /// <param name="to">To format</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> DateTime(string from, string to = null)
        {
            // In cases where we are reading from the database we get an object that
            // doesn't need a from formatter (e.g. a DateTime or Time)
            if (to == null)
            {
                to = from;
            }

            return delegate(object val, Dictionary<string, object> data)
            {
                DateTime dt;

                if (val == null || val as string == "" || val == DBNull.Value)
                {
                    return null;
                }

                if (val is DateTime)
                {
                    dt = (DateTime)val;
                }
                else if (val is TimeSpan)
                {
                    dt = new DateTime(1970, 1, 1);
                    dt = dt.Add((TimeSpan)val);
                }
                else
                {
                    var str = Convert.ToString(val);

                    dt = System.DateTime.ParseExact(str, from, System.Globalization.CultureInfo.InvariantCulture);
                }

                return dt.ToString(to);
            };
        }

        /// <summary>
        /// Convert a string of values into an array for use with checkboxes.
        /// </summary>
        /// <param name="delimiter">Delimiter to split on</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> Explode(string delimiter = "|")
        {
            return delegate(object val, Dictionary<string, object> data)
            {
                var str = (string)val;
                return str.Split(new[] { delimiter }, StringSplitOptions.None);
            };
        }

        /// <summary>
        /// Convert an array of values from a checkbox into a string which can be
        /// used to store in a text field in a database.
        /// </summary>
        /// <param name="delimiter">Delimiter to join on</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> Implode(string delimiter = "|")
        {
            return delegate(object val, Dictionary<string, object> data)
            {
                var str = (string[])val;
                return string.Join(delimiter, str);
            };
        }

        /// <summary>
        /// Convert an empty string to `null`. Null values are very useful in
        /// databases, but HTTP variables have no way of representing `null` as a
        /// value, often leading to an empty string and null overlapping. This method
        /// will check the value to operate on and return null if it is empty.
        /// </summary>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> NullEmpty()
        {
            return (val, data) => val.ToString() == "" ? null : val;
        }

        /// <summary>
        /// Convert an empty string to `null`. Null values are very useful in
        /// databases, but HTTP variables have no way of representing `null` as a
        /// value, often leading to an empty string and null overlapping. This method
        /// will check the value to operate on and return null if it is empty.
        /// </summary>
        /// <param name="emptyValue">Value to use if an empty value is submitted</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> IfEmpty(object emptyValue)
        {
            return (val, data) => val.ToString() == "" ? emptyValue : val;
        }

        /// <summary>
        /// Convert a number from using any character other than a period (dot) to
	    /// one which does use a period.This is useful for allowing numeric user
        /// input in regions where a comma is used as the decimal character. Use with
	    /// a set formatter.
        /// </summary>
        /// <param name="dec">Decimal place character</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> FromDecimalChar(char dec = ',')
        {
            return (val, data) => val.ToString().Replace(dec, '.');
        }

        /// <summary>
        /// Convert a number with a period (dot) as the decimal character to use
	    /// a different character(typically a comma). Use with a get formatter.
        /// </summary>
        /// <param name="dec">Decimal place character</param>
        /// <returns>Formatter delegate</returns>
        public static Func<object, Dictionary<string, object>, object> ToDecimalChar(char dec = ',')
        {
            return (val, data) => val.ToString().Replace('.', dec);
        }
    }
}
