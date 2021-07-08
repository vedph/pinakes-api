using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="BiblioController"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <exception cref="ArgumentNullException">client</exception>
        public BiblioController(ZoteroClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Gets the specified bibliography item.
        /// </summary>
        /// <returns>List of authors</returns>
        [HttpGet("api/biblio-items/{id}")]
        [ProducesResponseType(200)]
        public ActionResult<BiblioItem> GetItem([FromRoute] string id)
        {
            BiblioItem item = _client.GetItem(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Gets the specified set of bibliographic items.
        /// </summary>
        /// <param name="ids">The comma-delimited list of item IDs</param>
        /// <returns></returns>
        [HttpGet("api/biblio-items-set")]
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
    }
}
