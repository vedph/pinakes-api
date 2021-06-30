using Fusi.Tools.Data;
using System.Collections.Generic;

namespace Pinakes.Search
{
    /// <summary>
    /// Author search request.
    /// </summary>
    /// <seealso cref="PagingOptions" />
    public sealed class AuthorSearchRequest : PagingOptions
    {
        /// <summary>
        /// Gets or sets the text. This can include 1 or more tokens, separated
        /// by spaces. Each token is preceded by an optional operator; when no
        /// operator is specified, <c>*=</c> (contains) is assumed.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to match if at least any
        /// of the tokens in <see cref="Text"/> matches (i.e. tokens in OR
        /// relation). The default is to match all the tokens (AND), i.e.
        /// this property is false.
        /// </summary>
        public bool IsMatchAnyEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include author aliases
        /// in the search.
        /// </summary>
        public bool IncludeAlias { get; set; }

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
