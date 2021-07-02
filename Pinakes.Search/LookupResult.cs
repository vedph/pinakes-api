namespace Pinakes.Search
{
    /// <summary>
    /// A generic lookup result, with an ID and a string value representing it.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class LookupResult<TKey>
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the value.
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
