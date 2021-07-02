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
        [HttpPost("api/works")]
        [ProducesResponseType(200)]
        public ActionResult<DataPage<WorkResult>> GetWorks(
            [FromBody] WorkRequestModel model)
        {
            return Ok(_searcher.GetWorks(model.ToRequest()));
        }
    }
}
