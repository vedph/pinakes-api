using Pinakes.Search;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PinakesApi.Models
{
    /// <summary>
    /// Work search request model.
    /// </summary>
    public sealed class WorkRequestModel
    {
        /// <summary>
        /// The page number (1-N).
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        /// <summary>
        /// The size of the page (1-100).
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; }

        /// <summary>
        /// The query text. This can include 1 or more tokens, separated
        /// by spaces. Each token is preceded by an optional operator; when no
        /// operator is specified, <c>*=</c> (contains) is assumed.
        /// </summary>
        [MaxLength(200)]
        public string Text { get; set; }

        /// <summary>
        /// A value indicating whether to match if at least any
        /// of the tokens in <see cref="Text"/> matches (i.e. tokens in OR
        /// relation). The default is to match all the tokens (AND), i.e.
        /// this property is false.
        /// </summary>
        public bool IsMatchAnyEnabled { get; set; }

        /// <summary>
        /// The text search scope, represented by a comma delimited
        /// list of field codes from <c>wkttl</c>=work title, <c>wattl</c>=title
        /// alias, <c>wkinc</c>=incipit, <c>wkdes</c>=desinit, <c>wkplc</c>=place,
        /// <c>wknot</c>=note, <c>zwaut</c>=bibliography author, <c>zwttl</c>=
        /// bibliography title, <c>zwabs</c>=bibliography abstract.
        /// </summary>
        [MaxLength(100)]
        [RegularExpression("^(?:(?:wkttl|wattl|wktit|wkinc|wkdes|wkplc|wknot|zwaut|zwttl|zwabs),?)*$")]
        public string TextScope { get; set; }

        /// <summary>
        /// The author identifier; 0=do not filter by author ID.
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// The dictyon identifier. This is equal to the manuscript
        /// ID in the Pinakes DB; 0=do not filter by dictyon ID.
        /// </summary>
        public int DictyonId { get; set; }

        /// <summary>
        /// The minimum century value; 0=do not filter by century.
        /// </summary>
        public short CenturyMin { get; set; }

        /// <summary>
        /// The maximum century value; 0=do not filter by century.
        /// </summary>
        public short CenturyMax { get; set; }

        /// <summary>
        /// The IDs of the keywords to be matched, delimited by comma. At
        /// least one of the keywords should be matched.
        /// </summary>
        public string KeywordIds { get; set; }

        /// <summary>
        /// The IDs of the relation(s) to be matched as originating
        /// from the work being searched, delimited by comma. Any of the
        /// relations should match.
        /// </summary>
        public string RelationIds { get; set; }

        /// <summary>
        /// The target work identifier for the relation(s) matched
        /// by <see cref="RelationIds"/>.
        /// </summary>
        public int RelationTargetId { get; set; }

        /// <summary>
        /// The type of the external identifier to match for a work. For RAP
        /// purposes, the relevant identifiers are 3=TLG, 234=RAP,
        /// 120=Psellos, 139=Théologie byzantine II, 142=Regestes des actes du
        /// Patriarcat de Constantinople (see the <c>identifiants</c> table
        /// for the complete list).
        /// </summary>
        public int ExternalIdType { get; set; }

        /// <summary>
        /// The value of the external work identifier to match for a work.
        /// </summary>
        public string ExternalIdValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkRequestModel"/>
        /// class.
        /// </summary>
        public WorkRequestModel()
        {
            TextScope = "wkttl,wattl";
        }

        /// <summary>
        /// Converts this model to the corresponding search request.
        /// </summary>
        /// <returns>Request</returns>
        public WorkSearchRequest ToRequest()
        {
            return new WorkSearchRequest
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                Text = Text,
                TextScope = TextScope,
                IsMatchAnyEnabled = IsMatchAnyEnabled,
                AuthorId = AuthorId,
                DictyonId = DictyonId,
                CenturyMin = CenturyMin,
                CenturyMax = CenturyMax,
                KeywordIds = AuthorRequestModel.ParseIntIds(KeywordIds),
                RelationIds = AuthorRequestModel.ParseIntIds(RelationIds),
                RelationTargetId = RelationTargetId,
                ExternalIdType = ExternalIdType,
                ExternalIdValue = ExternalIdValue
            };
        }
    }
}
