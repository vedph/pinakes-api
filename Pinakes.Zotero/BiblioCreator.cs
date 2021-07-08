namespace Pinakes.Zotero
{
    /// <summary>
    /// Bibliographic item creator.
    /// </summary>
    public class BiblioCreator
    {
        /// <summary>
        /// Gets or sets the type (e.g. author).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{Type}] {LastName}, {FirstName}";
        }
    }
}
