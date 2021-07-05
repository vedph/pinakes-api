using Embix.Core.Filters;
using Embix.Search;
using Embix.Search.MySql;
using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Pinakes.Search
{
    /// <summary>
    /// Base class for Pinakes paged query builders. This provides shared
    /// functionality for <see cref="AuthorQueryBuilder"/> and
    /// <see cref="WorkQueryBuilder"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="MySqlPagedQueryBuilder&lt;TRequest&gt;" />
    public abstract class PinakesPagedQueryBuilder<TRequest>
        : MySqlPagedQueryBuilder<TRequest> where TRequest : TextBasedRequest
    {
        private readonly Regex _tokenRegex;
        private readonly QueryTextClauseBuilder _clauseBuilder;
        private readonly CompositeTextFilter _filter;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PinakesPagedQueryBuilder{TRequest}"/> class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        protected PinakesPagedQueryBuilder(string connString) : base(connString)
        {
            _tokenRegex = new Regex(@"^(?<o>=|<>|\*=|\^=|\$=|\?=|~=|%=)?(?<v>.+)");
            _clauseBuilder = new QueryTextClauseBuilder();
            _filter = new CompositeTextFilter(
                new WhitespaceTextFilter(),
                new StandardTextFilter());
        }

        /// <summary>
        /// Adds a text clause corresponding to the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="query">The query to receive the clause.</param>
        protected void AddTextClause(string token, Query query)
        {
            Match m = _tokenRegex.Match(token);
            if (!m.Success) return;     // defensive

            string op = m.Groups["o"].Length > 0 ? m.Groups["o"].Value : "=";
            string value = m.Groups["v"].Value;
            StringBuilder sb;

            switch (op)
            {
                // these operators require their text to be filtered
                case "=":
                case "<>":
                case "*=":
                case "^=":
                case "$=":
                    sb = new StringBuilder(value);
                    _filter.Apply(sb);
                    value = sb.ToString();
                    break;
            }
            if (value.Length > 0)
                _clauseBuilder.AddClause(query, "token.value", op, value);
        }

        /// <summary>
        /// Gets the non-text query, i.e. that part of the query which is not
        /// based on text search.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="nr">The ordinal number of the query to build.
        /// This is used to alias the query like t1, t2, etc.</param>
        /// <returns>The query.</returns>
        protected abstract Query GetNonTextQuery(TRequest request, int nr);

        /// <summary>
        /// Gets the Embix fields to search in from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Fields list.</returns>
        protected abstract IList<string> GetFields(TRequest request);

        /// <summary>
        /// Gets the count query connected to the specified IDs query.
        /// </summary>
        /// <param name="idQuery">The identifiers query.</param>
        /// <returns>The query.</returns>
        protected Query GetCountQuery(Query idQuery)
        {
            return QueryFactory.Query()
                .From(idQuery)
                .As("q")
                .AsCount(new[] { "q.id" });
        }

        /// <summary>
        /// Gets the data query connected to the specified IDs query.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="idQuery">The identifiers query.</param>
        /// <returns>The query.</returns>
        protected abstract Query GetDataQuery(TRequest request, Query idQuery);

        /// <summary>
        /// Builds the query from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Tuple where 1=query and 2=count query.</returns>
        /// <exception cref="ArgumentNullException">request</exception>
        public override Tuple<Query, Query> Build(TRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // normalize whitespace in text request, as we're going to split it
            // at each whitespace
            if (!string.IsNullOrEmpty(request.Text))
                request.Text = Regex.Replace(request.Text, @"\s+", " ").Trim();

            // create all the subqueries, one for each token
            List<Query> queries = new List<Query>();

            if (!string.IsNullOrEmpty(request.Text))
            {
                // text
                int n = 0;
                foreach (string token in request.Text.Split())
                {
                    Query tokenQuery = GetNonTextQuery(request, ++n);
                    tokenQuery.Join("occurrence", "occurrence.targetid", $"t{n}.id");
                    tokenQuery.Join("token", "occurrence.tokenid", "token.id");

                    IList<string> fields = GetFields(request);
                    if (fields.Count == 1)
                        tokenQuery.Where("occurrence.field", fields[0]);
                    else
                        tokenQuery.WhereIn("occurrence.field", fields);

                    AddTextClause(token, tokenQuery);
                    queries.Add(tokenQuery);
                }
            }
            else
            {
                // non-text
                queries.Add(GetNonTextQuery(request, 1));
            }

            Query idQuery;
            if (queries.Count > 1)
            {
                // MySql is a special case as it does not support intersect
                if (!request.IsMatchAnyEnabled
                    && QueryFactory.Compiler.GetType() == typeof(MySqlCompiler))
                {
                    idQuery = QueryFactory.Query("auteurs AS qs").As("q").Select("qs.id");
                    for (int i = 0; i < queries.Count; i++)
                    {
                        string alias = $"s{i}";
                        // idQuery.With(alias, queries[i]);
                        idQuery.Join(alias, "qs.id", $"{alias}.id");
                    }
                }
                else
                {
                    idQuery = queries[0].As("q");
                    for (int i = 1; i < queries.Count; i++)
                    {
                        if (request.IsMatchAnyEnabled) idQuery.Union(queries[i]);
                        else idQuery.Intersect(queries[i]);
                    }
                }
            }
            else idQuery = queries[0].As("q");

            var t = Tuple.Create(
                GetDataQuery(request, idQuery),
                GetCountQuery(idQuery));

            if (queries.Count > 1
                && !request.IsMatchAnyEnabled
                && QueryFactory.Compiler.GetType() == typeof(MySqlCompiler))
            {
                for (int i = 0; i < queries.Count; i++)
                {
                    string alias = $"s{i}";
                    t.Item1.With(alias, queries[i]);
                    t.Item2.With(alias, queries[i]);
                }
            }

#if DEBUG
            Debug.WriteLine("---ID:\n" + QueryFactory.Compiler.Compile(idQuery).ToString());
            Debug.WriteLine("\n---DATA:\n" +
                QueryFactory.Compiler.Compile(t.Item1).ToString());
            Debug.WriteLine("\n---COUNT:\n" +
                QueryFactory.Compiler.Compile(t.Item2).ToString());
            Debug.WriteLine("---");
#endif
            return t;
        }
    }
}
