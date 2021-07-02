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
        /// <returns>The query.</returns>
        protected override Query GetNonTextQuery(WorkSearchRequest request)
        {
            Query query = QueryFactory.Query("oeuvres AS t")
                .Select("t.id").Distinct();

            if (request.AuthorId > 0)
            {
                query.Join("oeuvres_auteurs wa", "t.id", "wa.id_oeuvre")
                    .Where("wa.id_auteur", request.AuthorId);
            }

            if (request.DictyonId > 0)
            {
                query.Join("temoins AS tm", "t.id", "temoins.id_oeuvre")
                    .Join("unites_codicologiques AS uc", "tm.id_uc", "uc.id")
                    .Where("uc.id_cote", request.DictyonId);
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
                query.Join("keywords_oeuvres AS kw", "t.id", "kw.id_oeuvre")
                    .WhereIn("kw.id_keyword", request.KeywordIds);
            }

            if (request.RelationIds?.Count > 0)
            {
                query.Join("relations AS r", "t.id", "id_parent")
                    .WhereIn("r.id_type", request.RelationIds);

                if (request.RelationTargetId > 0)
                    query.Where("r.id_child", request.RelationTargetId);
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
                .LeftJoin("oeuvres_auteurs AS wa", "oeuvres.id", "wa.id_oeuvre")
                .LeftJoin("auteurs AS a", "wa.id_auteur", "a.id")
                .LeftJoin("roles AS r", "wa.id_role", "r.id")
                // keywords
                .LeftJoin("keywords_oeuvres AS wk", "q.id", "wk.id_oeuvre")
                .LeftJoin("keywords AS k", "wk.id_keyword", "k.id")
                .Select("oeuvres.id",
                    "oeuvres.titre AS title",
                    "oeuvres.titulus",
                    "oeuvres.siecle AS century",
                    "oeuvres.dates",
                    "oeuvres.lieu AS place",
                    "oeuvres.remarque AS note",
                    // optional author(s)
                    "a.id AS authorId",
                    "a.nom AS authorName",
                    "wa.id_role AS authorRoleId",
                    "r.nom AS authorRoleName",
                    // optional keyword(s)
                    "k.id AS keywordId",
                    "k.keyword AS keywordValue")
                .OrderBy("a.nom", "oeuvres.titre", "oeuvres.id")
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
