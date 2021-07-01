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
