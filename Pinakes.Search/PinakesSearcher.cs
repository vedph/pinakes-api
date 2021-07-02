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
        private RelationQueryBuilder _relationQueryBuilder;
        private AuthorQueryBuilder _authorQueryBuilder;
        private WorkQueryBuilder _workQueryBuilder;

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
        public IList<LookupResult<int>> GetKeywords(bool authors)
        {
            if (_keywordQueryBuilder == null)
                _keywordQueryBuilder = new KeywordQueryBuilder(_connString);

            Query query = _keywordQueryBuilder.Build(authors);
            return query.Get<LookupResult<int>>().ToList();
        }

        /// <summary>
        /// Gets the list of relations.
        /// </summary>
        /// <param name="child">if set to <c>true</c> get the list on the child
        /// edge of the relations; otherwise, get the list on the parent's edge.
        /// </param>
        /// <returns>List of relations</returns>
        public IList<LookupResult<int>> GetRelations(bool child)
        {
            if (_relationQueryBuilder == null)
                _relationQueryBuilder = new RelationQueryBuilder(_connString);

            Query query = _relationQueryBuilder.Build(child);
            return query.Get<LookupResult<int>>().ToList();
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
            return new DataPage<AuthorResult>(request.PageNumber,
                request.PageSize, total, authors);
        }

        private static WorkResult GetWork(dynamic d)
        {
            return new WorkResult
            {
                Id = d.id,
                Title = d.title,
                Titulus = d.titulus,
                Century = d.century,
                Dates = d.dates,
                Place = d.place,
                Note = d.note
            };
        }

        /// <summary>
        /// Gets the works.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The works page.</returns>
        public DataPage<WorkResult> GetWorks(WorkSearchRequest request)
        {
            if (_workQueryBuilder == null)
                _workQueryBuilder = new WorkQueryBuilder(_connString);

            var t = _workQueryBuilder.Build(request);

            // count
            dynamic row = t.Item2.AsCount().First();
            int total = (int)row.count;

            // data
            if (total == 0)
            {
                return new DataPage<WorkResult>(
                    request.PageNumber,
                    request.PageSize,
                    0,
                    Array.Empty<WorkResult>());
            }

            List<WorkResult> works = new List<WorkResult>();
            WorkResult current = null;

            foreach (dynamic d in t.Item1.Get())
            {
                if (current == null || current.Id != d.id)
                {
                    // add pending work
                    if (current != null)
                    {
                        // TODO
                    }
                    current = GetWork(d);
                }
            }
            if (current != null)
            {
                // TODO
            }

            return new DataPage<WorkResult>(request.PageNumber,
                request.PageSize, total, works);
        }
    }
}
