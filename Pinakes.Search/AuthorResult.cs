using System.Collections.Generic;

namespace Pinakes.Search
{
    /// <summary>
    /// The author as resulting from a search.
    /// </summary>
    public class AuthorResult
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the aliases.
        /// </summary>
        public List<string> Aliases { get; set; }

        /// <summary>
        /// Gets or sets the century.
        /// </summary>
        public string Century { get; set; }

        /// <summary>
        /// Gets or sets the dates.
        /// </summary>
        public string Dates { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this record rather represents
        /// a category of authors.
        /// </summary>
        public bool IsCategory { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        public List<LookupResult<int>> Keywords { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"#{Id}: {Name}";
        }
    }
}
