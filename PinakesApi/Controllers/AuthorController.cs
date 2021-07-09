using Fusi.Tools.Data;
using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using PinakesApi.Models;

namespace PinakesApi.Controllers
{
    /// <summary>
    /// Authors search.
    /// </summary>
    /// <seealso cref="ControllerBase" />
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
        /// Gets the specified page of authors.
        /// </summary>
        /// <returns>List of authors</returns>
        [HttpPost("api/authors")]
        [ProducesResponseType(200)]
        public ActionResult<DataPage<AuthorResult>> GetAuthors(
            [FromBody] AuthorRequestModel model)
        {
            return Ok(_searcher.GetAuthors(model.ToRequest()));
        }

        /// <summary>
        /// Gets the details about the author with the specified ID.
        /// </summary>
        /// <returns>Author.</returns>
        [HttpGet("api/authors/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<AuthorResult> GetAuthor([FromRoute] int id)
        {
            AuthorResult author = _searcher.GetAuthorDetail(id);
            if (author == null) return NotFound();
            return Ok(author);
        }
    }
}
