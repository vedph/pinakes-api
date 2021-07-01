using SqlKata;
using System;
using System.Collections.Generic;

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
                // TODO

                if (request.RelationTargetId > 0)
                {
                    // TODO
                }
            }

            return query;
        }

        protected override Query GetDataQuery(WorkSearchRequest request,
            Query idQuery)
        {
            throw new NotImplementedException();
        }
    }
}
