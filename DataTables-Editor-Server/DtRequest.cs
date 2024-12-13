// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// DataTables and Editor request model
// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
#endif

namespace DataTables
{
    /// <summary>
    /// Representation of a DataTables or Editor request. This can be any form
    /// of request from the two libraries, including a standard DataTables get,
    /// a server-side processing request, or an Editor create, edit or delete
    /// command.
    /// </summary>
    public class DtRequest
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Static methods
         */

        /// <summary>
        /// Convert HTTP request data, in the standard HTTP parameter form
        /// submitted by jQuery into a generic dictionary of string / object
        /// pairs so the data can easily be accessed in .NET.
        ///
        /// This static method is generic and not specific to the DtRequest. It
        /// may be used for other data formats as well.
        /// 
        /// Note that currently this does not support nested arrays or objects in arrays
        /// </summary>
        /// <param name="dataIn">Collection of HTTP parameters sent by the client-side</param>
        /// <param name="cultureStr">Culture for locale specific conversions</param>
        /// <returns>Dictionary with the data and values contained. These may contain nested lists and dictionaries.</returns>
        public static Dictionary<string, object> HttpData(IEnumerable<KeyValuePair<string, string>> dataIn, string cultureStr = null)
        {
            var dataOut = new Dictionary<string, object>();
            CultureInfo culture = null;
            
            if (cultureStr != null) {
                culture = CultureInfo.CreateSpecificCulture(cultureStr);
            }

            if (dataIn != null)
            {
                foreach (var pair in dataIn)
                {
                    var value = _HttpConv(pair.Value, culture);

                    if (pair.Key.Contains("["))
                    {
                        var keys = pair.Key.Split(new[] {'['});
                        var innerDic = dataOut;
                        string key;

                        for (int i = 0, ien = keys.Count() - 1; i < ien; i++)
                        {
                            key = keys[i].TrimEnd(new[] {']'});
                            if (key == "")
                            {
                                // If the key is empty it is an array index value
                                key = innerDic.Count().ToString();
                            }

                            if (!innerDic.ContainsKey(key))
                            {
                                innerDic.Add(key, new Dictionary<string, object>());
                            }
                            innerDic = innerDic[key] as Dictionary<string, object>;
                        }

                        key = keys.Last().TrimEnd(new[] {']'});
                        if (key == "")
                        {
                            key = innerDic.Count().ToString();
                        }

                        innerDic.Add(key, value);
                    }
                    else
                    {
                        dataOut.Add(pair.Key, value);
                    }
                }
            }

            return dataOut;
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Public parameters
         */

        /// <summary>
        /// Type of request this instance contains the data for
        /// </summary>
        public RequestTypes RequestType;

        /// <summary>
        /// Request type values
        /// </summary>
        public enum RequestTypes
        {
            /// <summary>
            /// DataTables standard get for client-side processing
            /// </summary>
            DataTablesGet,

            /// <summary>
            /// DataTables server-side processing request
            /// </summary>
            DataTablesSsp,

            /// <summary>
            /// Editor create request
            /// </summary>
            EditorCreate,

            /// <summary>
            /// Editor edit request
            /// </summary>
            EditorEdit,

            /// <summary>
            /// Editor remove request
            /// </summary>
            EditorRemove,

            /// <summary>
            /// Editor file upload request
            /// </summary>
            EditorUpload,

            /// <summary>
            /// Editor dropdown search request
            /// </summary>
            EditorSearch
        };

        /* Server-side processing parameters */

        /// <summary>
        /// DataTables draw counter for server-side processing
        /// </summary>
        public int Draw;

        /// <summary>
        /// Search term for a dropdown search operation
        /// </summary>
        public string DropdownSearch;

        /// <summary>
        /// Search value for a dropdown search operation
        /// </summary>
        public List<string> DropdownValues;

        /// <summary>
        /// Field name for a dropdown search operation
        /// </summary>
        public string Field;

        /// <summary>
        /// DataTables record start pointer for server-side processing
        /// </summary>
        public int Start;

        /// <summary>
        /// DataTables page length parameter for server-side processing
        /// </summary>
        public int Length;

        /// <summary>
        /// Search information for server-side processing
        /// </summary>
        public SearchT Search = new SearchT();

        /// <summary>
        /// Column ordering information for server-side processing
        /// </summary>
        public List<OrderT> Order = new List<OrderT>();

        /// <summary>
        /// Column information for server-side processing
        /// </summary>
        public List<ColumnT> Columns = new List<ColumnT>();


        /* Editor parameters */

        /// <summary>
        /// Editor action request
        /// </summary>
        public string Action;

        /// <summary>
        /// Dictionary of data sent by Editor (may contain nested data)
        /// </summary>
        public Dictionary<string, object> Data;

        /// <summary>
        /// Information for searchPanes
        /// </summary>
        public Dictionary<string, string[]> searchPanes = new Dictionary<string, string[]>();

        /// <summary>
        /// Information for searchPanes_null
        /// </summary>
        public Dictionary<string, bool[]> searchPanes_null = new Dictionary<string, bool[]>();

        /// <summary>
        /// Information for searchPanes_null
        /// </summary>
        public SearchPanesOptions searchPanesOptions = new SearchPanesOptions();

        /// <summary>
        /// The last searchpane when dealing with cascade or viewTotal
        /// </summary>
        public String searchPanesLast = null;

        /// <summary>
        /// Information for searchBuilder
        /// </summary>
        public SearchBuilderDetails searchBuilder = new SearchBuilderDetails();

        /// <summary>
        /// List of ids for Editor to operate on
        /// </summary>
        public List<string> Ids = new List<string>();

        /// <summary>
        /// Upload field name
        /// </summary>
        public string UploadField;

        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructor
         */

#if NETCOREAPP
        public DtRequest(IEnumerable<KeyValuePair<String, StringValues>> rawHttp, string culture=null)
        {
            var raw = rawHttp.ToDictionary(x => x.Key, x => x.Value.ToString());
            _Build(raw, culture);
        }
#endif

        /// <summary>
        /// Convert an HTTP request submitted by the client-side into a
        /// DtRequest object
        /// </summary>
        /// <param name="rawHttp">Data from the client-side</param>
        public DtRequest(IEnumerable<KeyValuePair<string, string>> rawHttp, string culture=null)
        {
            _Build(rawHttp, culture);
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private functions
         */
        private static object _HttpConv(string dataIn, CultureInfo culture)
        {
            // Boolean
            if (dataIn == "true")
            {
                return true;
            }
            if (dataIn == "false")
            {
                return false;
            }

            // Numeric looking data, but with leading zero
            if (dataIn.Length > 1 && (dataIn.IndexOf('0') == 0 || dataIn.IndexOf('-') == dataIn.Length-1 || dataIn.IndexOf(',') != -1 || dataIn.IndexOf('+') == 0))
			{
                return dataIn;
            }

			int test;
			var res = Int32.TryParse(dataIn, out test);
            if (res)
			{
				return test;
			}

			decimal testDec;
			var resDec = culture != null
                ? Decimal.TryParse(dataIn, NumberStyles.AllowDecimalPoint, culture, out testDec)
                : Decimal.TryParse(dataIn, out testDec);
			if (resDec)
			{
				return testDec;
			}

            return dataIn;
        }
        
        private void _Build(IEnumerable<KeyValuePair<string, string>> rawHttp, string culture)
        {
            var http = HttpData(rawHttp, culture);

            if (http.ContainsKey("action"))
            {
                // Editor request
                Action = http["action"] as string;

                if (Action == "create")
                {
                    RequestType = RequestTypes.EditorCreate;
                    Data = http["data"] as Dictionary<string, object>;
                }
                else if (Action == "edit")
                {
                    RequestType = RequestTypes.EditorEdit;
                    Data = http["data"] as Dictionary<string, object>;
                }
                else if (Action == "remove")
                {
                    RequestType = RequestTypes.EditorRemove;
                    Data = http["data"] as Dictionary<string, object>;
                }
                else if (Action == "upload")
                {
                    RequestType = RequestTypes.EditorUpload;
                    UploadField = http["uploadField"] as string;
                }
                else if (Action == "search")
                {
                    RequestType = RequestTypes.EditorSearch;
                    Field = http["field"] as string;

                    if (http.ContainsKey("search"))
                    {
                        DropdownSearch = http["search"] as string;
                    }

                    if (http.ContainsKey("values"))
                    {
                        DropdownValues = new List<string>();

                        foreach (var item in http["values"] as Dictionary<string, object>)
                        {
                            DropdownValues.Add(item.Value as string);
                        }
                    }
                }
            }
            else if (http.ContainsKey("draw"))
            {
                // DataTables server-side processing get request
                RequestType = RequestTypes.DataTablesSsp;

                var search = http["search"] as Dictionary<string, object>;

                Draw = (int)http["draw"];
                Start = (int)http["start"];
                Length = (int)http["length"];
                Search = new SearchT
                {
                    Value = search["value"].ToString(),
                    Regex = (Boolean)search["regex"]
                };

                if (http.ContainsKey("order"))
                {
                    foreach (var item in http["order"] as Dictionary<string, object>)
                    {
                        var order = item.Value as Dictionary<string, object>;

                        Order.Add(new OrderT
                        {
                            Column = (int)order["column"],
                            Dir = order["dir"].ToString()
                        });
                    }
                }

                foreach (var item in http["columns"] as Dictionary<string, object>)
                {
                    var column = item.Value as Dictionary<string, object>;
                    var colSearch = column["search"] as Dictionary<string, object>;
                    Columns.Add(new ColumnT
                    {
                        // TODO
                        Name = column["name"].ToString(),
                        Data = column["data"].ToString(),
                        Searchable = (Boolean)column["searchable"],
                        Orderable = (Boolean)column["orderable"],
                        Search = new SearchT
                        {
                            Value = colSearch["value"].ToString(),
                            Regex = (Boolean)colSearch["regex"],
                        }
                    });
                }
                
                // SearchPanes
                if(http.ContainsKey("searchPanes")){
                        // Get the column names
                        Dictionary<string, object> httpSP = (Dictionary<string, object>) http["searchPanes"];
                        List<string> keyList = new List<string>(httpSP.Keys);

                        foreach(var key in keyList){
                            Dictionary<string, object> httpSPKey = (Dictionary<string, object>)httpSP[key];
                            List<string> keykeyList = new List<string>(httpSPKey.Keys);
                            string[] values = new string[keykeyList.Count()];
                            int count = 0;
                            foreach(var keykey in keykeyList){
                                values[count] = httpSPKey[keykey].ToString();
                                count++;
                            }

                            // Don't add multiple selections for one column
                            if(!searchPanes.ContainsKey(key)){
                                searchPanes.Add(key, values);
                            }
                            
                        }
                }

                // SearchPanes_null
                if(http.ContainsKey("searchPanes_null")){
                        // Get the column names
                        Dictionary<string, object> httpSP = (Dictionary<string, object>) http["searchPanes_null"];
                        List<string> keyList = new List<string>(httpSP.Keys);

                        foreach(var key in keyList){
                            Dictionary<string, object> httpSPKey = (Dictionary<string, object>)httpSP[key];
                            List<string> keykeyList = new List<string>(httpSPKey.Keys);
                            bool[] values = new bool[keykeyList.Count()];
                            int count = 0;
                            foreach(var keykey in keykeyList){
                                if(httpSPKey[keykey] is bool) {
                                    values[count] = (bool) httpSPKey[keykey];
                                    count++;
                                }
                            }

                            // Don't add multiple selections for one column
                            if(!searchPanes_null.ContainsKey(key)){
                                searchPanes_null.Add(key, values);
                            }
                            
                        }
                }
                // searchPanesLast
                if(http.ContainsKey("searchPanesLast")) {
                    searchPanesLast = (string)http["searchPanesLast"];
                }

                if (http.ContainsKey("searchPanes_options")) {
                    var options = (Dictionary<String, object>)http["searchPanes_options"];

                    searchPanesOptions.ViewCount = (Boolean)options["viewCount"];
                    searchPanesOptions.ViewTotal = (Boolean)options["viewTotal"];
                    searchPanesOptions.Cascade = (Boolean)options["cascade"];
                }

                // SearchBuilder
                if(http.ContainsKey("searchBuilder") && !(http["searchBuilder"] is String)){
                    searchBuilder = searchBuilderParse((Dictionary<String, object>)http["searchBuilder"]);
                }
            }
            else
            {
                // DataTables get request
                RequestType = RequestTypes.DataTablesGet;
            }
        }

        private SearchBuilderDetails searchBuilderParse(Dictionary<String, object> data) {
            var sb = new SearchBuilderDetails();
            // If the logic key is present then it must be a group and different parsing needs to occur
            if(data.ContainsKey("logic")) {
                sb.logic = (string)data["logic"];
                var criteria = (Dictionary<string, object>) data["criteria"];
                var keyList = new List<string>(criteria.Keys);

                foreach(var key in keyList) {
                    // Specifically the items in this group also need to be parsed
                    sb.criteria.Add( searchBuilderParse((Dictionary<string, object>)criteria[key]));
                }
            }
            // Otherwise if all of the values required to cause a search are present then make a criteria
            else if(data.ContainsKey("condition") && data.ContainsKey("origData")) {
                sb.condition = (String)data["condition"];
                sb.data = (String)data["data"];
                sb.origData = (String)data["origData"];
                sb.value1 = data.ContainsKey("value1") ? (String)data["value1"].ToString() : "";
                sb.value2 = data.ContainsKey("value2") ? (String)data["value2"].ToString() : "";
                sb.type = (String)data["type"];
            }

            return sb;
        }



        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Nested classes
         */

        /// <summary>
        /// Search class for server-side processing nested data
        /// </summary>
        public class SearchT
        {
            /// <summary>
            /// Search value
            /// </summary>
            public string Value;

            /// <summary>
            /// Regex flag
            /// </summary>
            public Boolean Regex;
        }

        /// <summary>
        /// Order class for server-side processing nested data
        /// </summary>
        public class OrderT
        {
            /// <summary>
            /// Column index
            /// </summary>
            public int Column;

            /// <summary>
            /// Ordering direction
            /// </summary>
            public string Dir;
        }

        /// <summary>
        /// Column class for server-side processing nested data
        /// </summary>
        public class ColumnT
        {
            /// <summary>
            /// Column data source property
            /// </summary>
            public string Data;

            /// <summary>
            /// Column name
            /// </summary>
            public string Name;

            /// <summary>
            /// Searchable flag
            /// </summary>
            public Boolean Searchable;

            /// <summary>
            /// Orderable flag
            /// </summary>
            public Boolean Orderable;

            /// <summary>
            /// Search term
            /// </summary>
            public SearchT Search;
        }

        /// <summary>
        /// Options for SearchPanes submitted by the client-side
        /// </summary>
        public class SearchPanesOptions
        {
            /// <summary>
            /// View count flag
            /// </summary>
            public Boolean ViewCount = true;

            /// <summary>
            /// View total flag
            /// </summary>
            public Boolean ViewTotal = false;

            /// <summary>
            /// Cascade flag
            /// </summary>
            public Boolean Cascade = false;
        }
    }
}
