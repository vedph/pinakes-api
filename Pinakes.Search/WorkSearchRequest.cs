using Fusi.Tools.Data;
using System.Collections.Generic;

namespace Pinakes.Search
{
    /// <summary>
    /// Work search request.
    /// </summary>
    /// <seealso cref="PagingOptions" />
    public class WorkSearchRequest : TextBasedRequest
    {
        /// <summary>
        /// Gets or sets the author identifier.
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Gets or sets the dictyon identifier. This is equal to the manuscript
        /// ID in the Pinakes DB; 0=do not filter by dictyon ID.
        /// </summary>
        public int DictyonId { get; set; }

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

        /// <summary>
        /// Gets or sets the IDs of the relation(s) to be matched as originating
        /// from the work being searched. Any of the relations should match.
        /// </summary>
        public List<int> RelationIds { get; set; }

        /// <summary>
        /// Gets or sets the target work identifier for the relation(s) matched
        /// by <see cref="RelationIds"/>.
        /// </summary>
        public int RelationTargetId { get; set; }

        /// <summary>
        /// Gets or sets the type of the external identifier to match for a
        /// work. For RAP purposes, the relevant identifiers are 3=TLG, 234=RAP,
        /// 120=Psellos, 139=Théologie byzantine II, 142=Regestes des actes du
        /// Patriarcat de Constantinople (see the <c>identifiants</c> table
        /// for the complete list).
        /// </summary>
        public int ExternalIdType { get; set; }

        /// <summary>
        /// Gets or sets the value of the external work identifier to match for
        /// a work.
        /// </summary>
        public string ExternalIdValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkSearchRequest"/> class.
        /// </summary>
        public WorkSearchRequest()
        {
            TextScope = "wkttl,wattl";
        }
    }
}
