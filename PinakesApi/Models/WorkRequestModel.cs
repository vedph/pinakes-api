using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PinakesApi.Models
{
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
        /// Gets or sets the text search scope, represented by a comma delimited
        /// list of field codes from <c>aunam</c>=author name, <c>aanam</c>=alias,
        /// <c>aunot</c>=note.
        /// </summary>
        [MaxLength(100)]
        [RegularExpression("^(?:(?:aunam|aanam|aunot),?)*$")]
        public string TextScope { get; set; }

        // TODO
    }
}
