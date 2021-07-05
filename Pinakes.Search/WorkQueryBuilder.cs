using SqlKata;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pinakes.Search
{
    /// <summary>
    /// Work search query builder.
    /// </summary>
    /// <seealso cref="PinakesPagedQueryBuilder&lt;WorkSearchRequest&gt;" />
    public sealed class WorkQueryBuilder :
        PinakesPagedQueryBuilder<WorkSearchRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public WorkQueryBuilder(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Gets the Embix fields to search in from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Fields list.</returns>
        protected override IList<string> GetFields(WorkSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.TextScope))
                return new[] { "wkttl", "wattl" };

            return request.TextScope.Split(',',
                StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the non-text query, i.e. that part of the query which is not
        /// based on text search.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="nr">The ordinal number of the query to build.
        /// This is used to alias the query like t1, t2, etc.</param>
        /// <returns>The query.</returns>
        protected override Query GetNonTextQuery(WorkSearchRequest request,
            int nr)
        {
            string tn = "t" + nr;
            Query query = QueryFactory.Query("oeuvres AS " + tn)
                .Select($"{tn}.id").Distinct();

            if (request.AuthorId > 0)
            {
                query.Join("oeuvres_auteurs", $"{tn}.id", "oeuvres_auteurs.id_oeuvre")
                    .Where("oeuvres_auteurs.id_auteur", request.AuthorId);
            }

            if (request.DictyonId > 0)
            {
                query.Join("temoins", $"{tn}.id", "temoins.id_oeuvre")
                    .Join("unites_codicologiques", "temoins.id_uc",
                        "unites_codicologiques.id")
                    .Where("unites_codicologiques.id_cote", request.DictyonId);
            }

            if (request.CenturyMin != 0)
            {
                query.Where("date.field", "wrk");
                query.Where("date.dateval", ">=", request.CenturyMin);
            }

            if (request.CenturyMax != 0)
            {
                query.Where("date.field", "wrk");
                query.Where("date.dateval", "<=", request.CenturyMax);
            }

            if (request.KeywordIds?.Count > 0)
            {
                query.Join("keywords_oeuvres", $"{tn}.id", "keywords_oeuvres.id_oeuvre")
                    .WhereIn("keywords_oeuvres.id_keyword", request.KeywordIds);
            }

            if (request.RelationIds?.Count > 0)
            {
                query.Join("relations", $"{tn}.id", "relations.id_parent")
                    .WhereIn("relations.id_type", request.RelationIds);

                if (request.RelationTargetId > 0)
                    query.Where("relations.id_child", request.RelationTargetId);
            }

            return query;
        }

        /// <summary>
        /// Gets the data query connected to the specified IDs query.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="idQuery">The identifiers query.</param>
        /// <returns>The query.</returns>
        protected override Query GetDataQuery(WorkSearchRequest request,
            Query idQuery)
        {
            // note that here we need to join works with other tables,
            // so that among the results a work might include more than 1 row.
            // It is up to the consumer consolidating them. This is more
            // performant than making a roundtrip to the server for each resulting
            // work, and these joins are performed only on the results page.

            Query query = QueryFactory.Query()
                .From(idQuery)
                .Join("oeuvres", "oeuvres.id", "q.id")
                // authors
                .LeftJoin("oeuvres_auteurs", "oeuvres.id", "oeuvres_auteurs.id_oeuvre")
                .LeftJoin("auteurs", "oeuvres_auteurs.id_auteur", "auteurs.id")
                .LeftJoin("roles", "oeuvres_auteurs.id_role", "roles.id")
                // keywords
                .LeftJoin("keywords_oeuvres", "q.id", "keywords_oeuvres.id_oeuvre")
                .LeftJoin("keywords", "keywords_oeuvres.id_keyword", "keywords.id")
                .Select("oeuvres.id",
                    "oeuvres.titre AS title",
                    "oeuvres.titulus",
                    "oeuvres.siecle AS century",
                    "oeuvres.date AS dates",
                    "oeuvres.lieu AS place",
                    "oeuvres.remarque AS note",
                    // optional author(s)
                    "auteurs.id AS authorId",
                    "auteurs.nom AS authorName",
                    "oeuvres_auteurs.id_role AS authorRoleId",
                    "roles.nom AS authorRoleName",
                    // optional keyword(s)
                    "keywords.id AS keywordId",
                    "keywords.keyword AS keywordValue")
                .OrderBy("auteurs.nom", "oeuvres.titre", "oeuvres.id")
                .Offset(request.GetSkipCount())
                .Limit(request.PageSize);

#if DEBUG
            Debug.WriteLine("---DATA:\n" +
                QueryFactory.Compiler.Compile(query).ToString());
#endif
            return query;
        }
    }
}
