using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using System.Collections.Generic;

namespace PinakesApi.Controllers
{
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly PinakesSearcher _searcher;

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
