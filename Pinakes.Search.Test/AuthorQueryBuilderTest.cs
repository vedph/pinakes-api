using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Pinakes.Search.Test
{
    public sealed class AuthorQueryBuilderTest
    {
        private readonly string _connString;
        private readonly MySqlCompiler _compiler;
        private readonly Regex _wsRegex;

        public AuthorQueryBuilderTest()
        {
            _connString = "Server=localhost;Database=pinakes;Uid=root;Pwd=mysql;";
            _compiler = new MySqlCompiler();
            _wsRegex = new Regex(@"\s+");
        }

        private string NormalizeWs(string text)
            => _wsRegex.Replace(text, " ").Trim();

        [Fact]
        public void Build_Text_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, `auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM " +
                "`auteurs` AS `t` " +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `occurrence`.`field` IN ('aunam', 'aanam') AND " +
                "LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he"
            });
            Assert.Equal(
                NormalizeWs(sql), 
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextPage2_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, `auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM " +
                "`auteurs` AS `t` " +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `occurrence`.`field` IN ('aunam', 'aanam') AND " +
                "LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20 OFFSET 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 2,
                PageSize = 20,
                Text = "*=he"
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextManyTokensAny_Ok()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Build_TextManyTokensAnd_Ok()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Build_TextScope_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, `auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM " +
                "`auteurs` AS `t` " +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `occurrence`.`field` IN ('aunam', 'aanam', 'aunot') AND " +
                "LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he",
                TextScope = "aunam,aanam,aunot"
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextCategory_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, " +
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM `auteurs` AS `t` " +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `t`.`is_categorie` = 1 " +
                "AND `occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` " +
                "ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he",
                IsCategory = true
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextCenturyMin_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, " +
                "`auteurs`.`siecle` AS `century`, `auteurs`.`dates`, " +
                "`auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM `auteurs` AS `t` " +
                "INNER JOIN `date` ON `date`.`targetid` = `t`.`id` " +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `date`.`field` = 'aut' AND `date`.`dateval` >= 15 " +
                "AND `occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he",
                CenturyMin = 15
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextCenturyMax_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, " +
                "`auteurs`.`siecle` AS `century`, `auteurs`.`dates`, " +
                "`auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM `auteurs` AS `t` " +
                "INNER JOIN `date` ON `date`.`targetid` = `t`.`id` " +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `date`.`field` = 'aut' AND `date`.`dateval` <= 15 " +
                "AND `occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he",
                CenturyMax = 15
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextKeywords_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs_alias`.`nom` AS `alias`, `auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t`.`id` FROM `auteurs` AS `t` " +
                "INNER JOIN `keywords_auteurs` ON " +
                "`t`.`id` = `keywords_auteurs`.`id_auteur`" +
                "INNER JOIN `occurrence` ON `occurrence`.`targetid` = `t`.`id` " +
                "INNER JOIN `token` ON `occurrence`.`tokenid` = `token`.`id` " +
                "WHERE `keywords_auteurs`.`id_keyword` IN (68) " +
                "AND `occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "LEFT JOIN `auteurs_alias` " +
                "ON `q`.`id` = `auteurs_alias`.`id_auteur` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he",
                KeywordIds = new List<int> { 68 }
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }
    }
}
