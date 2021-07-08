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
        /// <exception cref="ArgumentNullException">connString</exception>
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

        private static AuthorResult GetAuthor(dynamic d)
        {
            return new AuthorResult
            {
                Id = d.id,
                Name = d.name,
                Century = d.century ?? "",
                Dates = d.dates,
                Note = d.note,
                IsCategory = d.isCategory != null && d.isCategory
            };
        }

        private static void AddAliasToAuthor(dynamic d, AuthorResult author)
        {
            // nope if no alias
            if (d.alias == null) return;

            if (author.Aliases == null)
                author.Aliases = new List<string>();

            if (author.Aliases.All(a => a != d.alias))
                author.Aliases.Add(d.alias);
        }

        private static void AddKeywordToAuthor(dynamic d, AuthorResult author)
        {
            if (d.keywordId == null || d.keywordId == 0) return;

            if (author.Keywords == null)
                author.Keywords = new List<LookupResult<int>>();

            if (author.Keywords.All(k => k.Id != d.keywordId))
            {
                author.Keywords.Add(new LookupResult<int>
                {
                    Id = d.keywordId,
                    Value = d.keywordValue
                });
            }
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

            List<AuthorResult> authors = new List<AuthorResult>();
            AuthorResult author = null;

            foreach (dynamic d in t.Item1.Get())
            {
                if (author == null || author.Id != d.id)
                {
                    // add pending author
                    if (author != null) authors.Add(author);
                    author = GetAuthor(d);
                }
                AddAliasToAuthor(d, author);
            }
            if (author != null) authors.Add(author);

            return new DataPage<AuthorResult>(request.PageNumber,
                request.PageSize, total, authors);
        }

        /// <summary>
        /// Gets the details about the author with the specified ID.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <returns>Author or null if not found.</returns>
        public AuthorResult GetAuthorDetail(int id)
        {
            if (_authorQueryBuilder == null)
                _authorQueryBuilder = new AuthorQueryBuilder(_connString);

            Query query = _authorQueryBuilder.QueryFactory
                .Query("auteurs AS a")
                .LeftJoin("auteurs_alias AS aa", "a.id", "aa.id_auteur")
                .LeftJoin("keywords_auteurs AS ak", "a.id", "ak.id_auteur")
                .LeftJoin("keywords AS k", "ak.id_keyword", "k.id")
                .Select("a.id", "a.nom AS name", "a.siecle AS century",
                    "a.dates", "a.remarque AS note",
                    "a.is_categorie AS isCategory",
                    "aa.nom AS alias", "k.keyword AS keyword")
                .Where("a.id", id);

            AuthorResult author = null;
            foreach (dynamic d in query.Get())
            {
                if (author == null) author = GetAuthor(d);
                AddAliasToAuthor(d, author);
                AddKeywordToAuthor(d, author);
            }
            return author;
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

        private static void AddAuthorToWork(dynamic d, WorkResult work)
        {
            // nope if no author
            if (d.authorId == null || d.authorId == 0) return;

            if (work.Authors == null)
                work.Authors = new List<WorkResultAuthor>();

            if (work.Authors.All(a => a.Id != d.authorId))
            {
                work.Authors.Add(new WorkResultAuthor
                {
                    Id = d.id,
                    Name = d.authorName,
                    RoleId = d.roleId ?? 0,
                    Role = d.roleName
                });
            }
        }

        private static void AddKeywordToWork(dynamic d, WorkResult work)
        {
            if (d.keywordId == null || d.keywordId == 0) return;

            if (work.Keywords == null)
                work.Keywords = new List<LookupResult<int>>();

            if (work.Keywords.All(k => k.Id != d.keywordId))
            {
                work.Keywords.Add(new LookupResult<int>
                {
                    Id = d.keywordId,
                    Value = d.keywordValue
                });
            }
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
            WorkResult work = null;

            foreach (dynamic d in t.Item1.Get())
            {
                if (work == null || work.Id != d.id)
                {
                    // add pending work
                    if (work != null) works.Add(work);
                    work = GetWork(d);
                }
                AddAuthorToWork(d, work);
                AddKeywordToWork(d, work);
            }
            if (work != null) works.Add(work);

            return new DataPage<WorkResult>(request.PageNumber,
                request.PageSize, total, works);
        }

        public WorkResult GetWorkDetail(int id)
        {
            throw new NotImplementedException();
        }
    }
}
