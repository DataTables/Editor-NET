
// <summary>
// Field class which defines how individual fields for Editor
// </summary>
using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DataTables.EditorUtil;
using System.Web;

namespace DataTables
{
	/// <summary>
	/// Field definitions for the DataTables Editor.
	///
	/// Each Database column that is used with Editor can be described with this 
	/// Field method (both for Editor and Join instances). It basically tells
	/// Editor what table column to use, how to format the data and if you want
	/// to read and/or write this column.
	/// </summary>
	public class ColumnControl
	{
		public static void Ssp(Editor editor, Query query, DtRequest http)
		{
			foreach (var column in http.Columns)
			{
				if (column.ColumnControl != null)
				{
					var field = editor.Field(column.Data);

					// `<input>` based searches
					if (column.ColumnControl.Search != null)
					{
						var search = column.ColumnControl.Search;

						if (search.Type == "num")
						{
							_SspNumber(query, field, search.Value, search.Logic);
						}
						else if (search.Type == "date")
						{
							_SspDateTime(query, field, search.Value, search.Logic, search.Mask);
						}
						else
						{
							_SspText(query, field, search.Value, search.Logic);
						}
					}

					// SearchList
					if (column.ColumnControl.List != null)
					{
						var list = column.ColumnControl.List;

						query.WhereIn(field.DbField(), list);
					}
				}
			}
		}

		internal static void _SspDateTime(Query query, Field field, string value, string logic, string mask)
		{
			var bindingName = query.BindName();
			var dbField = field.DbField();
			var search = bindingName;

			// Only support date and time masks. This departs from the client side which allows
			// any component in the date/time to be masked out.
			if (mask == "YYYY-MM-DD")
			{
				dbField = "DATE(" + dbField + ")";
				search = "DATE(" + bindingName + ")";
			}
			else if (mask == "hh:mm:ss")
			{
				dbField = "TIME(" + dbField + ")";
				search = "TIME(" + bindingName + ")";
			}
			else
			{
				search = "(" + bindingName + ")";
			}

			if (logic == "empty")
			{
				query.Where(field.DbField(), null);
			}
			else if (logic == "notEmpty")
			{
				query.Where(field.DbField(), null, "!=");
			}
			else if (value == "")
			{
				// Empty search value means no search for the other logic operators
				return;
			}
			else if (logic == "equal")
			{
				query
					.Where(dbField, search, "=", false)
					.Bind(bindingName, value);
			}
			else if (logic == "notEqual")
			{
				query
					.Where(dbField, search, "!=", false)
					.Bind(bindingName, value);
			}
			else if (logic == "greater")
			{
				query
					.Where(dbField, search, ">", false)
					.Bind(bindingName, value);
			}
			else if (logic == "less")
			{
				query
					.Where(dbField, search, "<", false)
					.Bind(bindingName, value);
			}
		}

		internal static void _SspNumber(Query query, Field field, string value, string logic)
		{
			if (logic == "empty")
			{
				query.Where(q =>
				{
					q.Where(field.DbField(), null);
					q.OrWhere(field.DbField(), "");
				});
			}
			else if (logic == "notEmpty")
			{
				query.Where(q =>
				{
					q.Where(field.DbField(), null, "!=");
					q.Where(field.DbField(), "", "!=");
				});
			}
			else if (value == "")
			{
				// Empty search value means no search for the other logic operators
				return;
			}
			else if (logic == "equal")
			{
				query.Where(field.DbField(), value);
			}
			else if (logic == "notEqual")
			{
				query.Where(field.DbField(), value, "!=");
			}
			else if (logic == "greater")
			{
				query.Where(field.DbField(), value, ">");
			}
			else if (logic == "greaterOrEqual")
			{
				query.Where(field.DbField(), value, ">=");
			}
			else if (logic == "less")
			{
				query.Where(field.DbField(), value, "<");
			}
			else if (logic == "lessOrEqual")
			{
				query.Where(field.DbField(), value, "<=");
			}
		}

		internal static void _SspText(Query query, Field field, string value, string logic)
		{
			if (logic == "empty")
			{
				query.Where(q =>
				{
					q.Where(field.DbField(), null);
					q.OrWhere(field.DbField(), "");
				});
			}
			else if (logic == "notEmpty")
			{
				query.Where(q =>
				{
					q.Where(field.DbField(), null, "!=");
					q.Where(field.DbField(), "", "!=");
				});
			}
			else if (value == "")
			{
				// Empty search value means no search for the other logic operators
				return;
			}
			else if (logic == "equal")
			{
				query.Where(field.DbField(), value);
			}
			else if (logic == "notEqual")
			{
				query.Where(field.DbField(), value, "!=");
			}
			else if (logic == "contains")
			{
				query.Where(field.DbField(), "%" + value + "%", "like");
			}
			else if (logic == "notContains")
			{
				query.Where(field.DbField(), "%" + value + "%", "not like");
			}
			else if (logic == "starts")
			{
				query.Where(field.DbField(), value + "%", "like");
			}
			else if (logic == "ends")
			{
				query.Where(field.DbField(), "%" + value, "like");
			}
		}
	}
}
