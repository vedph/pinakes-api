using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using System.Collections.Generic;

namespace PinakesApi.Controllers
{
    [ApiController]
    public class KeywordController : ControllerBase
    {
        private readonly PinakesSearcher _searcher;

        public KeywordController(PinakesSearcher searcher)
        {
            _searcher = searcher;
        }

        [HttpGet("api/keywords/authors")]
        [ProducesResponseType(200)]
        public ActionResult<IList<KeywordResult>> GetAuthorKeywords()
        {
            return Ok(_searcher.GetKeywords(true));
        }

        [HttpGet("api/keywords/works")]
        [ProducesResponseType(200)]
        public ActionResult<IList<KeywordResult>> GetWorkKeywords()
        {
            return Ok(_searcher.GetKeywords(false));
        }
    }
}
