using Microsoft.AspNetCore.Mvc;
using Pinakes.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
