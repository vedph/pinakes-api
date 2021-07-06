﻿using System;
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
        public string GetItem(string id)
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

        /// <summary>
        /// Gets the data fragment for the specified item.
        /// </summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="forAuthor">if set to <c>true</c>, the fragment is being
        /// retrieved for authors; else for works.</param>
        /// <returns>The fragment or null if not found.</returns>
        /// <exception cref="ArgumentNullException">id</exception>
        public BiblioItemFragment GetItemFragment(string id, bool forAuthor)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string json = GetItem(id);
            if (json == null) return null;

            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement data = doc.RootElement.GetProperty("data");
            BiblioItemFragment fr = new BiblioItemFragment
            {
                Id = data.GetProperty("key").GetString(),
                IsAuthorTarget = forAuthor,
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
                sb.Append(data.GetProperty("lastName").GetString());
                sb.Append(", ");
                sb.Append(data.GetProperty("firstName").GetString());
            }
            fr.Authors = sb.ToString();
            return fr;
        }
    }
}