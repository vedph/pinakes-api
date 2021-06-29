using Fusi.Antiquity.Chronology;
using Xunit;

namespace Pinakes.Index.Test
{
    public sealed class PinakesCenturyDateAdapterTest
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("invalid", null)]
        [InlineData("1657-1719", null)]
        [InlineData("09", "IX AD")]
        [InlineData("12 (1/2)", "c. 1125 AD")]
        [InlineData("12 (2/2)", "c. 1150 AD")]
        [InlineData("12 (1/3)", "c. 1116 AD")]
        [InlineData("12 (2/3)", "c. 1133 AD")]
        [InlineData("12 (3/3)", "c. 1149 AD")]
        [InlineData("12 (1/4)", "c. 1112 AD")]
        [InlineData("12 (2/4)", "c. 1125 AD")]
        [InlineData("12 (3/4)", "c. 1137 AD")]
        [InlineData("12 (4/4)", "c. 1150 AD")]
        public void GetDate_Ok(string text, string expected)
        {
            PinakesCenturyDateAdapter adapter = new PinakesCenturyDateAdapter();
            HistoricalDate date = adapter.GetDate(text);
            if (expected == null) Assert.Null(date);
            else Assert.Equal(expected, date.ToString());
        }
    }
}
