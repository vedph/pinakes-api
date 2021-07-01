using Fusi.Tools.Data;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pinakes.Search
{
    /// <summary>
    /// Main entry point for search in Pinakes RAP.
    /// </summary>
    public sealed class PinakesSearcher
    {
        private readonly string _connString;
        private KeywordQueryBuilder _keywordQueryBuilder;
        private AuthorQueryBuilder _authorQueryBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PinakesSearcher"/> class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        /// <exception cref="System.ArgumentNullException">connString</exception>
        public PinakesSearcher(string connString)
        {
            _connString = connString
                ?? throw new ArgumentNullException(nameof(connString));
        }

        /// <summary>
        /// Gets the list of keywords for authors or works.
        /// </summary>
        /// <param name="authors">if set to <c>true</c>, get the authors keywords;
        /// else get the works keywords.</param>
        /// <returns>List of keywords</returns>
        public IList<KeywordResult> GetKeywords(bool authors)
        {
            if (_keywordQueryBuilder == null)
                _keywordQueryBuilder = new KeywordQueryBuilder(_connString);

            Query query = _keywordQueryBuilder.Build(authors);
            return query.Get<KeywordResult>().ToList();
        }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The authors page.</returns>
        public DataPage<AuthorResult> GetAuthors(AuthorSearchRequest request)
        {
            if (_authorQueryBuilder == null)
                _authorQueryBuilder = new AuthorQueryBuilder(_connString);

            var t = _authorQueryBuilder.Build(request);

            // count
            dynamic row = t.Item2.AsCount().First();
            int total = (int)row.count;

            // data
            if (total == 0)
            {
                return new DataPage<AuthorResult>(
                    request.PageNumber,
                    request.PageSize,
                    0,
                    Array.Empty<AuthorResult>());
            }

            List<AuthorResult> authors = t.Item1.Get<AuthorResult>().ToList();
            return new DataPage<AuthorResult>(
                request.PageNumber,
                request.PageSize,
                total,
                authors);
        }

        public DataPage<WorkResult> GetWorks(WorkSearchRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
