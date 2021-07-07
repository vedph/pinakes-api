using Fusi.Tools.Data;
using System.Collections.Generic;

namespace Pinakes.Search
{
    /// <summary>
    /// Author search request.
    /// </summary>
    /// <seealso cref="PagingOptions" />
    public sealed class AuthorSearchRequest : TextBasedRequest
    {
        /// <summary>
        /// Gets or sets the data set identifier. When greater than 0, this
        /// represents the id_type from table identifiants, used as a marker
        /// for a specific subset of Pinakes. For RAP, this ID is 234.
        /// </summary>
        public int SetId { get; set; }

        /// <summary>
        /// Gets or sets the category flag to be matched for the author.
        /// </summary>
        public bool? IsCategory { get; set; }

        /// <summary>
        /// Gets or sets the minimum century value.
        /// </summary>
        public short CenturyMin { get; set; }

        /// <summary>
        /// Gets or sets the maximum century value.
        /// </summary>
        public short CenturyMax { get; set; }

        /// <summary>
        /// Gets or sets the IDs of the keywords to be matched. At least one
        /// of the keywords should be matched.
        /// </summary>
        public List<int> KeywordIds { get; set; }
    }
}
