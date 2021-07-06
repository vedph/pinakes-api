namespace Pinakes.Zotero
{
    /// <summary>
    /// The fragment of a Zotero bibliographic item data required for indexing.
    /// </summary>
    public class BiblioItemFragment
    {
        /// <summary>
        /// Gets or sets the Zotero item identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this fragment targets an
        /// author (true) or a work (false).
        /// </summary>
        public bool IsAuthorTarget { get; set; }

        /// <summary>
        /// Gets or sets the target author/work identifier.
        /// </summary>
        public int TargetId { get; set; }

        /// <summary>
        /// Gets or sets the authors. Each author has the form <c>last, first</c>,
        /// and several authors are separated by semicolon.
        /// </summary>
        public string Authors { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the optional abstract.
        /// </summary>
        public string Abstract { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{Id}] {(IsAuthorTarget? 'A' : 'W')}#{TargetId} {Authors} - {Title}";
        }
    }
}
