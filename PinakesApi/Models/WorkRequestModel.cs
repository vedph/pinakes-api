﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        /// The text search scope, represented by a comma delimited
        /// list of field codes from <c>aunam</c>=author name, <c>aanam</c>=alias,
        /// <c>aunot</c>=note.
        /// </summary>
        [MaxLength(100)]
        [RegularExpression("^(?:(?:wkttl|wattl|wktit|wkinc|wkdes|wkplc|wknot),?)*$")]
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
        /// The minimum century value.
        /// </summary>
        public short CenturyMin { get; set; }

        /// <summary>
        /// The maximum century value.
        /// </summary>
        public short CenturyMax { get; set; }

        /// <summary>
        /// The IDs of the keywords to be matched. At least one
        /// of the keywords should be matched.
        /// </summary>
        public List<int> KeywordIds { get; set; }

        /// <summary>
        /// The IDs of the relation(s) to be matched as originating
        /// from the work being searched. Any of the relations should match.
        /// </summary>
        public List<int> RelationIds { get; set; }

        /// <summary>
        /// The target work identifier for the relation(s) matched
        /// by <see cref="RelationIds"/>.
        /// </summary>
        public int RelationTargetId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkRequestModel"/> class.
        /// </summary>
        public WorkRequestModel()
        {
            TextScope = "wkttl,wattl";
        }
    }
}