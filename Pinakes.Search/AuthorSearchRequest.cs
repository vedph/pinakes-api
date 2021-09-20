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
        /// The type of the external identifier to match for an author's work.
        /// For RAP purposes, the relevant identifiers are 3=TLG, 234=RAP,
        /// 120=Psellos, 139=Théologie byzantine II, 142=Regestes des actes du
        /// Patriarcat de Constantinople (see the <c>identifiants</c> table
        /// for the complete list). This is a filter for authors, but it refers
        /// to works as it is the works, not the authors, to be marked for
        /// a specific subset of the database.
        /// </summary>
        public int ExternalIdType { get; set; }

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
