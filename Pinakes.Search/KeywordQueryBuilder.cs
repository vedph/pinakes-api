using Embix.Search.MySql;
using SqlKata;

namespace Pinakes.Search
{
    /// <summary>
    /// Query builder for getting the full list of keywords related to authors
    /// or works.
    /// </summary>
    /// <seealso cref="MySqlNonPagedQueryBuilder&lt;bool&gt;" />
    public sealed class KeywordQueryBuilder : MySqlNonPagedQueryBuilder<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeywordQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public KeywordQueryBuilder(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Builds the query from the specified request.
        /// </summary>
        /// <param name="request">Get keywords for authors if true, for works
        /// if false.</param>
        /// <returns>Query.</returns>
        public override Query Build(bool request)
        {
            Query subQuery = QueryFactory.Query(request
                ? "keywords_auteurs AS kl"
                : "keywords_oeuvres AS kl")
            .Where("kl.id", "k.id");

            return QueryFactory.Query("keywords AS k")
                .Select("id", "keyword")
                .WhereExists(subQuery)
                .OrderBy("keyword");
        }
    }
}
