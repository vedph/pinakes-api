using Fusi.Tools.Data;
using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using PinakesApi.Models;
using System;

namespace PinakesApi.Controllers
{
    /// <summary>
    /// Works search.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public sealed class WorkController : ControllerBase
    {
        private readonly PinakesSearcher _searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkController"/> class.
        /// </summary>
        /// <param name="searcher">The searcher.</param>
        /// <exception cref="ArgumentNullException">searcher</exception>
        public WorkController(PinakesSearcher searcher)
        {
            _searcher = searcher
                ?? throw new ArgumentNullException(nameof(searcher));
        }

        /// <summary>
        /// Gets the specified page of works.
        /// </summary>
        /// <returns>List of works</returns>
        [HttpGet("api/works")]
        [ProducesResponseType(200)]
        public ActionResult<DataPage<WorkResult>> GetWorks(
            [FromQuery] WorkRequestModel model)
        {
            return Ok(_searcher.GetWorks(model.ToRequest()));
        }

        /// <summary>
        /// Gets the details about the work with the specified ID.
        /// </summary>
        /// <returns>Work.</returns>
        [HttpGet("api/works/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<WorkDetailResult> GetWork([FromRoute] int id)
        {
            WorkDetailResult work = _searcher.GetWorkDetail(id);
            if (work == null) return NotFound();
            return Ok(work);
        }
    }
}
