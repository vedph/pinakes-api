namespace Pinakes.Search
{
    /// <summary>
    /// Details about a work.
    /// </summary>
    /// <seealso cref="WorkResult" />
    public class WorkDetailResult : WorkResult
    {
        /// <summary>
        /// Gets or sets the incipit.
        /// </summary>
        public string Incipit { get; set; }

        /// <summary>
        /// Gets or sets the desinit.
        /// </summary>
        public string Desinit { get; set; }

        /// <summary>
        /// Gets or sets the note about dates.
        /// </summary>
        public string DatesNote { get; set; }

        /// <summary>
        /// Gets or sets the note about place.
        /// </summary>
        public string PlaceNote { get; set; }

        /// <summary>
        /// Gets or sets the person in charge for this entry.
        /// </summary>
        public string Manager { get; set; }

        /// <summary>
        /// Gets or sets the reference team.
        /// </summary>
        public string Team { get; set; }
    }
}
