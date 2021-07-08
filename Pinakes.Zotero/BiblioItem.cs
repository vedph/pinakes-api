using System.Collections.Generic;
using System.Text;

namespace Pinakes.Zotero
{
    /// <summary>
    /// Selected data about a Zotero bibliographic item.
    /// </summary>
    public class BiblioItem
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the item's key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the type (e.g. book).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the creators.
        /// </summary>
        public List<BiblioCreator> Creators { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the abstract.
        /// </summary>
        public string Abstract { get; set; }

        /// <summary>
        /// Gets or sets the series the item eventually belongs to.
        /// </summary>
        public string Series { get; set; }

        /// <summary>
        /// Gets or sets the series number.
        /// </summary>
        public string SeriesNumber { get; set; }

        /// <summary>
        /// Gets or sets the edition number.
        /// </summary>
        public short Edition { get; set; }

        /// <summary>
        /// Gets or sets the publication place.
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// Gets or sets the publisher name.
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the publication year.
        /// </summary>
        public short Year { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the ISBN code.
        /// </summary>
        public string Isbn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiblioItem"/> class.
        /// </summary>
        public BiblioItem()
        {
            Creators = new List<BiblioCreator>();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[').Append(Type).Append(']');

            if (Creators?.Count > 0)
                sb.Append(' ').Append(string.Join("; ", Creators));

            sb.Append(" - ").Append(Title);
            sb.Append(" - ").Append(Place).Append(" (").Append(Year).Append(')');

            return sb.ToString();
        }
    }
}
