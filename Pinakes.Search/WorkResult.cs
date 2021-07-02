using System.Collections.Generic;

namespace Pinakes.Search
{
    /// <summary>
    /// A result of a works search.
    /// </summary>
    public class WorkResult
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the authors.
        /// </summary>
        public List<WorkResultAuthor> Authors { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the titulus.
        /// </summary>
        public string Titulus { get; set; }

        /// <summary>
        /// Gets or sets the century.
        /// </summary>
        public string Century { get; set; }

        /// <summary>
        /// Gets or sets the dates.
        /// </summary>
        public string Dates { get; set; }

        /// <summary>
        /// Gets or sets the place.
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string Note { get; set; }

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
            return $"#{Id}: {Title}";
        }
    }

    /// <summary>
    /// An author of a <see cref="WorkResult"/>.
    /// </summary>
    public class WorkResultAuthor
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
        /// Gets or sets the role identifier.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"#{Id} {Name}";
        }
    }
}
