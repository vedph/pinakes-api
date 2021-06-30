using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using System.Collections.Generic;

namespace PinakesApi.Controllers
{
    /// <summary>
    /// Authors search.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly PinakesSearcher _searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorController"/> class.
        /// </summary>
        /// <param name="searcher">The searcher.</param>
        public AuthorController(PinakesSearcher searcher)
        {
            _searcher = searcher;
        }

        /// <summary>
        /// Gets all the keywords related to authors.
        /// </summary>
        /// <returns>List of keywords</returns>
        [HttpPost("api/authors")]
        [ProducesResponseType(200)]
        public ActionResult<IList<KeywordResult>> GetAuthors(
            [FromBody] AuthorRequestModel model)
        {
            return Ok(_searcher.GetAuthors(model.ToRequest()));
        }
    }
}
