using Embix.Search.MySql;
using SqlKata;

namespace Pinakes.Search
{
    /// <summary>
    /// Relations lookup query builder.
    /// </summary>
    /// <seealso cref="MySqlNonPagedQueryBuilder&lt;bool&gt;" />
    public sealed class RelationQueryBuilder : MySqlNonPagedQueryBuilder<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeywordQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public RelationQueryBuilder(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Builds the query from the specified request.
        /// </summary>
        /// <param name="request">Get keywords for child role if true, for parent
        /// role if false.</param>
        /// <returns>Query.</returns>
        public override Query Build(bool request)
        {
            string vf = request ? "child_role" : "parent_role";
            return QueryFactory.Query("relations_types AS rt")
                .Select("id", $"rt.{vf} AS value")
                .OrderBy($"rt.{vf}");
        }
    }
}
