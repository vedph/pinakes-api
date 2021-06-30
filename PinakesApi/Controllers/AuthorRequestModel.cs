using Pinakes.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PinakesApi.Controllers
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
        /// A value indicating whether to match if at least any
        /// of the tokens in <see cref="Text"/> matches (i.e. tokens in OR
        /// relation). The default is to match all the tokens (AND), i.e.
        /// this property is false.
        /// </summary>
        public bool IsMatchAnyEnabled { get; set; }

        /// <summary>
        /// A value indicating whether to include author aliases
        /// in the search.
        /// </summary>
        public bool IncludeAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include author notes.
        /// </summary>
        public bool IncludeNotes { get; set; }

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
        /// The IDs of the keywords to be matched. At least one of the keywords
        /// should be matched.
        /// </summary>
        public List<int> KeywordIds { get; set; }

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
                IsMatchAnyEnabled = IsMatchAnyEnabled,
                IncludeAlias = IncludeAlias,
                IncludeNotes = IncludeNotes,
                IsCategory = IsCategory,
                CenturyMin = CenturyMin,
                CenturyMax = CenturyMax
            };
        }
    }
}
