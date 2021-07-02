using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using System.Collections.Generic;

namespace PinakesApi.Controllers
{
    /// <summary>
    /// Relations lookup.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public class RelationController : ControllerBase
    {
        private readonly PinakesSearcher _searcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationController"/>
        /// class.
        /// </summary>
        /// <param name="searcher">The searcher.</param>
        public RelationController(PinakesSearcher searcher)
        {
            _searcher = searcher;
        }

        /// <summary>
        /// Gets all the relations on the parent's edge.
        /// </summary>
        /// <returns>List of relations</returns>
        [HttpGet("api/relations/parent")]
        [ProducesResponseType(200)]
        public ActionResult<IList<LookupResult<int>>> GetAuthorRelations()
        {
            return Ok(_searcher.GetRelations(false));
        }

        /// <summary>
        /// Gets all the relations on the child's edge.
        /// </summary>
        /// <returns>List of relations</returns>
        [HttpGet("api/relations/child")]
        [ProducesResponseType(200)]
        public ActionResult<IList<LookupResult<int>>> GetWorkRelations()
        {
            return Ok(_searcher.GetRelations(true));
        }
    }
}
