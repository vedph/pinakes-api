using Pinakes.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PinakesApi.Models
{
    /// <summary>
    /// The model for an author search request.
    /// </summary>
    public sealed class AuthorRequestModel
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
        /// The text search scope, represented by a comma delimited
        /// list of field codes from <c>aunam</c>=author name, <c>aanam</c>=alias,
        /// <c>aunot</c>=note, <c>zaaut</c>=bibliography author, <c>zattl</c>=
        /// bibliography title, <c>zaabs</c>=bibliography abstract.
        /// </summary>
        [MaxLength(100)]
        [RegularExpression("^(?:(?:aunam|aanam|aunot|zaaut|zattl|zaabs),?)*$")]
        public string TextScope { get; set; }

        /// <summary>
        /// A value indicating whether to match if at least any
        /// of the tokens in <see cref="Text"/> matches (i.e. tokens in OR
        /// relation). The default is to match all the tokens (AND), i.e.
        /// this property is false.
        /// </summary>
        public bool IsMatchAnyEnabled { get; set; }

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
        /// The category flag to be matched for the author.
        /// </summary>
        public bool? IsCategory { get; set; }

        /// <summary>
        /// The minimum century value, 0 if disabled.
        /// </summary>
        public short CenturyMin { get; set; }

        /// <summary>
        /// The maximum century value, 0 if disabled.
        /// </summary>
        public short CenturyMax { get; set; }

        /// <summary>
        /// The IDs of the keywords to be matched, comma-separated. At least
        /// one of the keywords should be matched.
        /// </summary>
        public string KeywordIds { get; set; }

        internal static List<int> ParseIntIds(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            List<int> ids = new List<int>();
            foreach (string s in text.Split(",",
                StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(s, out int id))
                    ids.Add(id);
            }
            return ids.Count > 0 ? ids : null;
        }

        /// <summary>
        /// Converts this model to the corresponding request.
        /// </summary>
        /// <returns>Request.</returns>
        public AuthorSearchRequest ToRequest()
        {
            return new AuthorSearchRequest
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                Text = Text,
                TextScope = TextScope,
                IsMatchAnyEnabled = IsMatchAnyEnabled,
                ExternalIdType = ExternalIdType,
                IsCategory = IsCategory,
                CenturyMin = CenturyMin,
                CenturyMax = CenturyMax,
                KeywordIds = ParseIntIds(KeywordIds)
            };
        }
    }
}
