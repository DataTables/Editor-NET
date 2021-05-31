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
            EditorUpload
        };

        /* Server-side processing parameters */

        /// <summary>
        /// DataTables draw counter for server-side processing
        /// </summary>
        public int Draw;

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

                foreach (var item in http["order"] as Dictionary<string, object>)
                {
                    var order = item.Value as Dictionary<string, object>;

                    Order.Add(new OrderT
                    {
                        Column = (int)order["column"],
                        Dir = order["dir"].ToString()
                    });
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
            }
            else
            {
                // DataTables get request
                RequestType = RequestTypes.DataTablesGet;
            }
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
    }
}
