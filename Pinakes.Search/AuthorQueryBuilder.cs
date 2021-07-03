﻿using Embix.Search.MySql;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pinakes.Search
{
    /// <summary>
    /// Authors search MySql query builder.
    /// </summary>
    /// <seealso cref="MySqlPagedQueryBuilder&lt;AuthorSearchRequest&gt;" />
    public sealed class AuthorQueryBuilder :
        PinakesPagedQueryBuilder<AuthorSearchRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public AuthorQueryBuilder(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Gets the non-text query, i.e. that part of the query which is not
        /// based on text search.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The query.</returns>
        protected override Query GetNonTextQuery(AuthorSearchRequest request)
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
                query.Join("keywords_auteurs AS ka", "t.id", "ka.id_auteur")
                    .WhereIn("ka.id_keyword", request.KeywordIds);
            }

            return query;
        }

        /// <summary>
        /// Gets the data query connected to the specified IDs query.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="idQuery">The identifiers query.</param>
        /// <returns>The query.</returns>
        protected override Query GetDataQuery(AuthorSearchRequest request,
            Query idQuery)
        {
            Query query = QueryFactory.Query()
                .From(idQuery)
                .Join("auteurs", "auteurs.id", "q.id")
                .LeftJoin("auteurs_alias", "q.id", "auteurs_alias.id_auteur")
                .Select("auteurs.id",
                    "auteurs.nom AS name",
                    "auteurs_alias.nom AS alias",
                    "auteurs.siecle AS century",
                    "auteurs.dates",
                    "auteurs.remarque AS note",
                    "auteurs.is_categorie AS isCategory")
                .OrderBy("auteurs.nom", "auteurs.id")
                .Offset(request.GetSkipCount())
                .Limit(request.PageSize);

#if DEBUG
            Debug.WriteLine("---DATA:\n" + 
                QueryFactory.Compiler.Compile(query).ToString());
#endif
            return query;
        }

        /// <summary>
        /// Gets the Embix fields to search in from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Fields list.</returns>
        protected override IList<string> GetFields(AuthorSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.TextScope))
                return new[] { "aunam", "aanam" };

            return request.TextScope.Split(',',
                StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
