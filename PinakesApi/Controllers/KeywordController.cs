using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using System.Collections.Generic;

namespace PinakesApi.Controllers
{
    /// <summary>
    /// Keywords lookup.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public class KeywordController : ControllerBase
    {
        private readonly PinakesSearcher _searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeywordController"/>
        /// class.
        /// </summary>
        /// <param name="searcher">The searcher.</param>
        public KeywordController(PinakesSearcher searcher)
        {
            _searcher = searcher;
        }

        /// <summary>
        /// Gets all the keywords related to authors.
        /// </summary>
        /// <returns>List of keywords</returns>
        [HttpGet("api/keywords/authors")]
        [ProducesResponseType(200)]
        public ActionResult<IList<KeywordResult>> GetAuthorKeywords()
        {
            return Ok(_searcher.GetKeywords(true));
        }

        /// <summary>
        /// Gets the all the keywords related to works.
        /// </summary>
        /// <returns>List of keywords</returns>
        [HttpGet("api/keywords/works")]
        [ProducesResponseType(200)]
        public ActionResult<IList<KeywordResult>> GetWorkKeywords()
        {
            return Ok(_searcher.GetKeywords(false));
        }
    }
}
