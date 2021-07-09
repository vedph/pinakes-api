using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using Pinakes.Zotero;
using System;
using System.Collections.Generic;

namespace PinakesApi.Controllers
{
    /// <summary>
    /// Bibliography controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public sealed class BiblioController : ControllerBase
    {
        private readonly ZoteroClient _client;
        private readonly PinakesSearcher _searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiblioController"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="searcher">The searcher.</param>
        /// <exception cref="ArgumentNullException">client</exception>
        public BiblioController(ZoteroClient client, PinakesSearcher searcher)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _searcher = searcher ?? throw new ArgumentNullException(nameof(searcher));
        }

        /// <summary>
        /// Gets the bibliography item with the specified ID.
        /// </summary>
        /// <returns>List of authors</returns>
        [HttpGet("api/biblio/items/{id}")]
        [ProducesResponseType(200)]
        public ActionResult<BiblioItem> GetItem([FromRoute] string id)
        {
            BiblioItem item = _client.GetItem(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Gets the set of bibliographic items having the specified IDs.
        /// </summary>
        /// <param name="ids">The comma-delimited list of item IDs</param>
        /// <returns></returns>
        [HttpGet("api/biblio/items-set")]
        public ActionResult<BiblioItem[]> GetItems([FromQuery] string ids)
        {
            List<BiblioItem> items = new List<BiblioItem>();
            foreach (string id in ids.Split(',',
                StringSplitOptions.RemoveEmptyEntries))
            {
                BiblioItem item = _client.GetItem(id);
                if (item != null) items.Add(item);
            }
            return Ok(items.ToArray());
        }

        /// <summary>
        /// Gets the bibliography items for the author with the specified ID.
        /// </summary>
        /// <param name="id">The author's identifier.</param>
        /// <returns>The author's items.</returns>
        [HttpGet("api/biblio/authors/{id}")]
        public ActionResult<BiblioItem[]> GetAuthorItems([FromRoute] int id)
        {
            List<BiblioItem> items = new List<BiblioItem>();
            foreach (string zid in _searcher.GetAuthorBiblioItemIds(id, 234))
            {
                BiblioItem item = _client.GetItem(zid);
                if (item != null) items.Add(item);
            }
            return Ok(items.ToArray());
        }

        /// <summary>
        /// Gets the bibliography items for the work with the specified ID.
        /// </summary>
        /// <param name="id">The work's identifier.</param>
        /// <returns>The work's items.</returns>
        [HttpGet("api/biblio/works/{id}")]
        public ActionResult<BiblioItem[]> GetWorkItems([FromRoute] int id)
        {
            List<BiblioItem> items = new List<BiblioItem>();
            foreach (string zid in _searcher.GetWorkBiblioItemIds(id, 234))
            {
                BiblioItem item = _client.GetItem(zid);
                if (item != null) items.Add(item);
            }
            return Ok(items.ToArray());
        }
    }
}
