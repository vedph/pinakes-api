using Embix.Search.MySql;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;

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
        /// <param name="nr">The ordinal number of the query to build.
        /// This is used to alias the query like t1, t2, etc.</param>
        /// <returns>The query.</returns>
        protected override Query GetNonTextQuery(AuthorSearchRequest request,
            int nr)
        {
            string tn = "t" + nr;
            Query query = QueryFactory.Query("auteurs AS " + tn)
                .Select($"{tn}.id").Distinct();

            if (request.ExternalIdType > 0)
            {
                query.Join("oeuvres_auteurs AS wa", $"{tn}.id", "wa.id_auteur")
                    .Join("identifiants_oeuvres AS wi", "wi.id_oeuvre", "wa.id_oeuvre")
                    .Join("identifiants AS i", "wi.id_identifiant", "i.id")
                    .Where("i.id_type", request.ExternalIdType);
            }

            if (request.IsCategory.HasValue)
            {
                query.Where($"{tn}.is_categorie",
                    request.IsCategory.Value ? 1 : 0);
            }

            if (request.CenturyMin != 0)
            {
                query.Join("pix_date", $"{tn}.id", "pix_date.target_id");
                query.Where("pix_date.field", "aut");
                query.Where("pix_date.date_val", ">=",
                    WorkQueryBuilder.GetCenturyTresholdValue(
                        request.CenturyMin, false));
            }

            if (request.CenturyMax != 0)
            {
                if (request.CenturyMin == 0)
                    query.Join("pix_date", $"{tn}.id", "pix_date.target_id");
                query.Where("pix_date.field", "aut");
                query.Where("pix_date.date_val", "<=",
                    WorkQueryBuilder.GetCenturyTresholdValue(
                        request.CenturyMax, true));
            }

            if (request.KeywordIds?.Count > 0)
            {
                query.Join("keywords_auteurs", $"{tn}.id",
                    "keywords_auteurs.id_auteur")
                    .WhereIn("keywords_auteurs.id_keyword", request.KeywordIds);
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
            Query result = QueryFactory.Query()
                .From(idQuery)
                .As("q")
                .Join("auteurs", "auteurs.id", "q.id")
                // .LeftJoin("auteurs_alias", "q.id", "auteurs_alias.id_auteur")
                .Select("auteurs.id",
                    "auteurs.nom AS name",
                    // "auteurs_alias.nom AS alias",
                    "auteurs.siecle AS century",
                    "auteurs.dates",
                    "auteurs.remarque AS note",
                    "auteurs.is_categorie AS isCategory")
                .OrderBy("auteurs.nom", "auteurs.id");

            if (request.PageSize > 0)
            {
                result.Offset(request.GetSkipCount())
                      .Limit(request.PageSize);
            }
            return result;
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
