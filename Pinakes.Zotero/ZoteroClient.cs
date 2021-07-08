using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Pinakes.Zotero
{
    /// <summary>
    /// Zotero client for Pinakes.
    /// </summary>
    public sealed class ZoteroClient
    {
        private readonly string _key;

        /// <summary>
        /// Gets or sets the group identifier. The default is 669969 i.e.
        /// ihrt_grec.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoteroClient"/> class.
        /// </summary>
        /// <param name="key">The key to use for requests.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        public ZoteroClient(string key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            GroupId = 669969;
        }

        /// <summary>
        /// Gets the item with the specified Zotero ID.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The JSON code representing the item, or null if not found.
        /// </returns>
        /// <exception cref="ArgumentNullException">id</exception>
        public string GetItemJson(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("Zotero-API-Version", "3");
                client.Headers.Add("Authentication", "Bearer " + _key);
                return client.DownloadString(
                    $"https://api.zotero.org/groups/{GroupId}/items/" + id);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        private static string GetOptionalString(JsonElement element, string name) 
            => element.TryGetProperty(name, out JsonElement e) ? e.GetString() : null;

        private static short GetOptionalInt16(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out JsonElement e)) return 0;
            string value = e.GetString();
            if (value != null && short.TryParse(value, out short n)) return n;
            return 0;
        }

        /// <summary>
        /// Gets the data fragment for the specified item.
        /// </summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="targetId">The target ID.</param>
        /// <param name="forAuthor">if set to <c>true</c>, the fragment is being
        /// retrieved for authors; else for works.</param>
        /// <returns>The fragment or null if not found.</returns>
        /// <exception cref="ArgumentNullException">id</exception>
        public BiblioItemFragment GetItemFragment(string id, int targetId,
            bool forAuthor)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string json = GetItemJson(id);
            if (json == null) return null;

            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement data = doc.RootElement.GetProperty("data");
            BiblioItemFragment fr = new BiblioItemFragment
            {
                ZoteroId = data.GetProperty("key").GetString(),
                IsAuthorTarget = forAuthor,
                TargetId = targetId,
                Title = data.GetProperty("title").GetString(),
                Abstract = GetOptionalString(data, "abstractNote")
            };

            // authors
            StringBuilder sb = new StringBuilder();
            foreach (JsonElement author in data.GetProperty("creators")
                .EnumerateArray())
            {
                if (GetOptionalString(author, "creatorType") != "author") continue;
                if (sb.Length > 0) sb.Append("; ");
                sb.Append(author.GetProperty("lastName").GetString());
                sb.Append(", ");
                sb.Append(author.GetProperty("firstName").GetString());
            }
            fr.Authors = sb.ToString();
            return fr;
        }

        /// <summary>
        /// Gets the item with the specified ID.
        /// </summary>
        /// <param name="id">The item identifier.</param>
        /// <returns>The item or null if not found.</returns>
        /// <exception cref="ArgumentNullException">id</exception>
        public BiblioItem GetItem(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            string json = GetItemJson(id);
            if (json == null) return null;

            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement data = doc.RootElement.GetProperty("data");
            BiblioItem item = new BiblioItem
            {
                GroupId = GroupId,
                Key = data.GetProperty("key").GetString(),
                Type = data.GetProperty("itemType").GetString(),
                Title = data.GetProperty("title").GetString(),
                Abstract = GetOptionalString(data, "abstractNote"),
                Series = GetOptionalString(data, "series"),
                SeriesNumber = GetOptionalString(data, "seriesNumber"),
                Edition = GetOptionalInt16(data, "edition"),
                Place = GetOptionalString(data, "place"),
                Publisher = GetOptionalString(data, "publisher"),
                Year = GetOptionalInt16(data, "date"),
                Language = GetOptionalString(data, "language"),
                Isbn = GetOptionalString(data, "ISBN")
            };

            // creators
            foreach (JsonElement creator in data.GetProperty("creators")
                .EnumerateArray())
            {
                item.Creators.Add(new BiblioCreator
                {
                    Type = GetOptionalString(creator, "creatorType"),
                    LastName = creator.GetProperty("lastName").GetString(),
                    FirstName = creator.GetProperty("firstName").GetString()
                });
            }

            return item;
        }
    }
}
