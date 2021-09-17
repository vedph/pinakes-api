using SqlKata.Compilers;
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
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM " +
                "`auteurs` AS `t1` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam') AND " +
                "LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM " +
                "`auteurs` AS `t1` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam') AND " +
                "LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` FROM (" +
                "SELECT DISTINCT `t1`.`id` FROM `auteurs` AS `t1` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%he%' " +
                "UNION SELECT DISTINCT `t2`.`id` FROM `auteurs` AS `t2` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t2`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%an%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he *=an",
                IsMatchAnyEnabled = true
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextManyTokensAnd_Ok()
        {
            const string sql =
                "WITH `s0` AS (SELECT DISTINCT `t1`.`id` FROM `auteurs` AS `t1` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%he%'), " +
                "`s1` AS (SELECT DISTINCT `t2`.`id` FROM `auteurs` AS `t2` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t2`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%an%') " +
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` FROM (" +
                "SELECT `qs`.`id` FROM `auteurs` AS `qs` " +
                "INNER JOIN `s0` ON `qs`.`id` = `s0`.`id` " +
                "INNER JOIN `s1` ON `qs`.`id` = `s1`.`id`) AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
                "ORDER BY `auteurs`.`nom`, `auteurs`.`id` LIMIT 20";

            AuthorQueryBuilder builder = new AuthorQueryBuilder(_connString);
            var t = builder.Build(new AuthorSearchRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Text = "*=he *=an",
                IsMatchAnyEnabled = false
            });
            Assert.Equal(
                NormalizeWs(sql),
                NormalizeWs(_compiler.Compile(t.Item1).ToString()));
        }

        [Fact]
        public void Build_TextScope_Ok()
        {
            const string sql =
                "SELECT `auteurs`.`id`, `auteurs`.`nom` AS `name`, " +
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM " +
                "`auteurs` AS `t1` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `eix_occurrence`.`field` IN ('aunam', 'aanam', 'aunot') AND " +
                "LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM `auteurs` AS `t1` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `t1`.`is_categorie` = 1 " +
                "AND `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
                "`auteurs`.`siecle` AS `century`, `auteurs`.`dates`, " +
                "`auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM `auteurs` AS `t1` " +
                "INNER JOIN `pix_date` ON `t1`.`id` = `pix_date`.`target_id` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `pix_date`.`field` = 'aut' AND `pix_date`.`date_val` >= 1400 " +
                "AND `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
                "`auteurs`.`siecle` AS `century`, `auteurs`.`dates`, " +
                "`auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM `auteurs` AS `t1` " +
                "INNER JOIN `pix_date` ON `t1`.`id` = `pix_date`.`target_id` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `pix_date`.`field` = 'aut' AND `pix_date`.`date_val` <= 1499 " +
                "AND `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
                "`auteurs`.`siecle` AS `century`, " +
                "`auteurs`.`dates`, `auteurs`.`remarque` AS `note`, " +
                "`auteurs`.`is_categorie` AS `isCategory` " +
                "FROM (SELECT DISTINCT `t1`.`id` FROM `auteurs` AS `t1` " +
                "INNER JOIN `keywords_auteurs` ON " +
                "`t1`.`id` = `keywords_auteurs`.`id_auteur` " +
                "INNER JOIN `eix_occurrence` ON `eix_occurrence`.`target_id` = `t1`.`id` " +
                "INNER JOIN `eix_token` ON `eix_occurrence`.`token_id` = `eix_token`.`id` " +
                "WHERE `keywords_auteurs`.`id_keyword` IN (68) " +
                "AND `eix_occurrence`.`field` IN ('aunam', 'aanam') " +
                "AND LOWER(`eix_token`.`value`) like '%he%') AS `q` " +
                "INNER JOIN `auteurs` ON `auteurs`.`id` = `q`.`id` " +
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
