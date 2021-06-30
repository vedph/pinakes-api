using Embix.Search.MySql;
using SqlKata;
using SqlKata.Execution;
using System;

namespace Pinakes.Search
{
    /// <summary>
    /// Authors search MySql query builder.
    /// </summary>
    /// <seealso cref="MySqlPagedQueryBuilder&lt;AuthorSearchRequest&gt;" />
    public sealed class AuthorQueryBuilder : MySqlPagedQueryBuilder<AuthorSearchRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public AuthorQueryBuilder(string connString) : base(connString)
        {
        }

        private void ApplyNonTextFilters(AuthorSearchRequest request,
            Query query)
        {
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
                    .Where("ak.id_auteur", "auteurs.id")
                    .WhereIn("ak.id_keyword", request.KeywordIds));
            }
        }

        /// <summary>
        /// Builds the query from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Tuple where 1=query and 2=count query.</returns>
        /// <exception cref="ArgumentNullException">request</exception>
        public override Tuple<Query, Query> Build(AuthorSearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Query query = QueryFactory.Query("")

            throw new NotImplementedException();
        }
    }
}
