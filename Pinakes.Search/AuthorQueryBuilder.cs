﻿using Embix.Core.Filters;
using Embix.Search;
using Embix.Search.MySql;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Pinakes.Search
{
    /// <summary>
    /// Authors search MySql query builder.
    /// </summary>
    /// <seealso cref="MySqlPagedQueryBuilder&lt;AuthorSearchRequest&gt;" />
    public sealed class AuthorQueryBuilder : MySqlPagedQueryBuilder<AuthorSearchRequest>
    {
        private readonly Regex _tokenRegex;
        private readonly QueryTextClauseBuilder _clauseBuilder;
        private readonly CompositeTextFilter _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public AuthorQueryBuilder(string connString) : base(connString)
        {
            _tokenRegex = new Regex(@"^(?<o>=|<>|\*=|\^=|\$=|\?=|~=|%=)?(?<v>.+)");
            _clauseBuilder = new QueryTextClauseBuilder();
            _filter = new CompositeTextFilter(
                new WhitespaceTextFilter(),
                new StandardTextFilter());
        }

        private void AddTextClause(string token, Query query)
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

        private Query GetNonTextQuery(AuthorSearchRequest request)
        {
            Query query = QueryFactory.Query("auteurs AS t")
                .Select("t.id").Distinct();

            if (request.IsCategory.HasValue)
            {
                query.Where("auteurs.is_categorie",
                    request.IsCategory.Value ? 1 : 0);
            }

            if (request.CenturyMin != 0)
            {
                query.Where("date.field", "aut");
                query.Where("date.dateval", ">=", request.CenturyMin);
            }

            if (request.CenturyMax != 0)
            {
                query.Where("date.field", "aut");
                query.Where("date.dateval", "<=", request.CenturyMax);
            }

            if (request.KeywordIds?.Count > 0)
            {
                // where exists (select id_keyword from keywords_auteurs
                //   where ak.id_auteur=auteurs.id AND ak.id_keyword IN...)
                query.WhereExists(QueryFactory.Query("keywords_auteurs AS ak")
                    .Select("id_keyword")
                    .Where("ak.id_auteur", "t.id")
                    .WhereIn("ak.id_keyword", request.KeywordIds));
            }

            return query;
        }

        private Query GetCountQuery(Query idQuery)
        {
            Query query = QueryFactory.Query().From(idQuery).AsCount(new[] { "q.id" });
#if DEBUG
            Debug.WriteLine("---COUNT:\n" + QueryFactory.Compiler.Compile(query).ToString());
#endif
            return query;
        }

        private Query GetResultQuery(AuthorSearchRequest request, Query idQuery)
        {
            Query query = QueryFactory.Query()
                .From(idQuery)
                .Join("auteurs", "auteurs.id", "q.id")
                .Select("auteurs.id",
                    "auteurs.nom AS name",
                    "auteurs.siecle AS century",
                    "auteurs.dates",
                    "auteurs.remarque AS note",
                    "auteurs.is_categorie AS iscategory")
                .OrderBy("auteurs.nom", "auteurs.id")
                .Offset(request.GetSkipCount())
                .Limit(request.PageSize);

#if DEBUG
            Debug.WriteLine("---DATA:\n" + QueryFactory.Compiler.Compile(query).ToString());
#endif
            return query;
        }

        private static IList<string> GetFields(AuthorSearchRequest request)
        {
            List<string> fields = new List<string> { "aunam" };
            if (request.IncludeAlias) fields.Add("aanam");
            if (request.IncludeNotes) fields.Add("aunot");
            return fields;
        }

        /// <summary>
        /// Builds the query from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Tuple where 1=query and 2=count query.</returns>
        /// <exception cref="ArgumentNullException">request</exception>
        public override Tuple<Query, Query> Build(AuthorSearchRequest request)
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
                foreach (string token in request.Text.Split())
                {
                    Query tokenQuery = GetNonTextQuery(request);
                    tokenQuery.Join("occurrence", "occurrence.targetid", "t.id");
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
                queries.Add(GetNonTextQuery(request));
            }

            // build a concatenated query
            Query idQuery = queries[0].As("q");
            if (queries.Count > 1)
            {
                for (int i = 1; i < queries.Count; i++)
                {
                    if (request.IsMatchAnyEnabled) idQuery.Union(queries[i]);
                    else idQuery.Intersect(queries[i]);
                }
            }

#if DEBUG
            Debug.WriteLine("---ID:\n" + QueryFactory.Compiler.Compile(idQuery).ToString());
#endif

            return Tuple.Create(
                GetResultQuery(request, idQuery),
                GetCountQuery(idQuery));
        }
    }
}
