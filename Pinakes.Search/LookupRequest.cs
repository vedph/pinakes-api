namespace Pinakes.Search
{
    /// <summary>
    /// A generic text-based lookup request.
    /// </summary>
    public class LookupRequest
    {
        /// <summary>
        /// Gets or sets the text to be matched.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the max count of matching results to be returned.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupRequest"/> class.
        /// </summary>
        public LookupRequest()
        {
            Limit = 20;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Text} ({Limit})";
        }
    }
}
