using System;
using System.Collections.Generic;

namespace DataTables
{
    // This class is used when working with SearchBuilder to represent the JSON that is passed to the server side
    public class SearchBuilderDetails
    {
        public String condition = null;

        public String data = null;

        public String origData = null;

        public String value1 = null;

        public String value2 = null;

        public List<SearchBuilderDetails> criteria = new List<SearchBuilderDetails>();
    
        public String logic = null;
    }
}