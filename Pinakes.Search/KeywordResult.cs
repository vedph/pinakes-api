namespace Pinakes.Search
{
    /// <summary>
    /// A keywords lookup result.
    /// </summary>
    public class KeywordResult
    {
        /// <summary>
        /// Gets or sets the kwyword identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the keyword value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"#{Id}: {Value}";
        }
    }
}
