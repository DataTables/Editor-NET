// <copyright>Copyright (c) 2014 SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Where class that defines information needed to construct a where query
// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTables.DatabaseUtil
{
    internal class Where
    {
        private string _field = null;
        private string _group = null;
        private string _operator = null;
        private string _query = null;


        public string Field()
        {
            return _field;
        }

        public Where Field(string value)
        {
            _field = value;
            return this;
        }


        public string Group()
        {
            return _group;
        }

        public Where Group(string value)
        {
            _group = value;
            return this;
        }


        public string Operator()
        {
            return _operator;
        }

        public Where Operator(string value)
        {
            _operator = value;
            return this;
        }


        public string Query()
        {
            return _query;
        }

        public Where Query(string value)
        {
            _query = value;
            return this;
        }
    }
}
