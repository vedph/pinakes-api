using Fusi.Tools.Data;

namespace Pinakes.Search
{
    /// <summary>
    /// A search request based on text search.
    /// </summary>
    /// <seealso cref="PagingOptions" />
    public class TextBasedRequest : PagingOptions
    {
        /// <summary>
        /// Gets or sets the text. This can include 1 or more tokens, separated
        /// by spaces. Each token is preceded by an optional operator; when no
        /// operator is specified, <c>*=</c> (contains) is assumed.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the search scope for <see cref="Text"/>
        /// represented by a comma-delimited list of field codes.
        /// </summary>
        public string TextScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to match if at least any
        /// of the tokens in <see cref="Text"/> matches (i.e. tokens in OR
        /// relation). The default is to match all the tokens (AND), i.e.
        /// this property is false.
        /// </summary>
        public bool IsMatchAnyEnabled { get; set; }
    }
}
