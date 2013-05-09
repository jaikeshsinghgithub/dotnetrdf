using System;
using System.Collections.Generic;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Query.Builder
{
    sealed class DescribeBuilder : IDescribeBuilder
    {
        readonly List<IToken> _describeVariables = new List<IToken>();

        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        public IDescribeBuilder And(params string[] variables)
        {
            foreach (var variableName in variables)
            {
                _describeVariables.Add(new VariableToken(variableName, 0, 0, 0));   
            }
            return this;
        }

        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        public IDescribeBuilder And(params Uri[] uris)
        {
            foreach (var uri in uris)
            {
                _describeVariables.Add(new UriToken(string.Format("<{0}>", uri), 0, 0, 0));
            }
            return this;
        }

        public IQueryBuilder GetQueryBuilder()
        {
            return new QueryBuilder(this);
        }

        internal SparqlQueryType SparqlQueryType
        {
            get { return SparqlQueryType.Describe; }
        }

        internal IEnumerable<IToken> DescribeVariables
        {
            get { return _describeVariables; }
        }
    }
}