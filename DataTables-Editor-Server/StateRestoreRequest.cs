// <copyright>Copyright (c) SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// StateRestore request class
// </summary>
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
#endif

namespace DataTables
{
    /// <summary>
    /// Representation of a StateRestore request.
    /// </summary>
    public class StateRestoreRequest
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* Public properties
		*/
        public string Action = "";
        public string Id = "";
        public List<string> Ids = new List<string>();
        public Boolean IsDefault = false;
        public Boolean IsSharedOut = false;
        public string Name = "";
        public string Path = "";
        public string State = "";
        public string Table = "";


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Constructor
         */

#if NETCOREAPP
        public StateRestoreRequest(
            IEnumerable<KeyValuePair<String, StringValues>> rawHttp,
            string culture = null
        )
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
        public StateRestoreRequest(IEnumerable<KeyValuePair<string, string>> rawHttp, string culture = null)
        {
            _Build(rawHttp, culture);
        }


        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Private methods
         */
        private void _Build(IEnumerable<KeyValuePair<string, string>> rawHttp, string culture = null)
        {
            var http = DtRequest.HttpData(rawHttp, culture);

            if (http.ContainsKey("action"))
            {
                Action = http["action"] as string;
            }

            if (http.ContainsKey("id"))
            {
                Id = http["id"].ToString();
            }

            if (http.ContainsKey("ids"))
            {
                foreach (var id in http["ids"] as Dictionary<string, object>)
                {
                    System.Console.WriteLine(id.Value as string);
                    Ids.Add(id.Value.ToString());
                }
            }

            if (http.ContainsKey("isDefault"))
            {
                IsDefault = (Boolean)http["isDefault"];
            }

            if (http.ContainsKey("isSharedOut"))
            {
                IsSharedOut = (Boolean)http["isSharedOut"];
            }

            if (http.ContainsKey("name"))
            {
                Name = http["name"] as string;
            }

            if (http.ContainsKey("path"))
            {
                Path = http["path"] as string;
            }

            if (http.ContainsKey("state"))
            {
                State = http["state"] as string;
            }

            if (http.ContainsKey("table"))
            {
                Table = http["table"] as string;
            }
        }
    }
}