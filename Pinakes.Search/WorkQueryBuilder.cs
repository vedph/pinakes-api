using Embix.Core.Filters;
using Embix.Search;
using Embix.Search.MySql;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pinakes.Search
{
    public sealed class WorkQueryBuilder : PinakesPagedQueryBuilder<WorkSearchRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkQueryBuilder"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        private WorkQueryBuilder(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Gets the Embix fields to search in from the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Fields list.</returns>
        protected override IList<string> GetFields(WorkSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.TextScope))
                return new[] { "wkttl", "wkals" };

            return request.TextScope.Split(',',
                StringSplitOptions.RemoveEmptyEntries);
        }

        protected override Query GetCountQuery(Query idQuery)
        {
            throw new NotImplementedException();
        }

        protected override Query GetDataQuery(WorkSearchRequest request, Query idQuery)
        {
            throw new NotImplementedException();
        }

        protected override Query GetNonTextQuery(WorkSearchRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
