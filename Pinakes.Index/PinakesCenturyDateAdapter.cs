using Fusi.Antiquity.Chronology;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Pinakes.Index
{
    /// <summary>
    /// Pinakes century expressions to <see cref="HistoricalDate"/> adapter.
    /// </summary>
    public sealed class PinakesCenturyDateAdapter
    {
        private readonly Regex _yearRegex;
        private readonly Regex _centRegex;

        public PinakesCenturyDateAdapter()
        {
            _yearRegex = new Regex(@"\d{4}");

            // v = value
            // ie = in or ex
            // fn, fd = fraction numerator and denominator
            _centRegex = new Regex(
                @"\s*(?<v>\d{1,2})\s*(?:(?:(?<ie>in|ex)\.?)|" +
                @"(?:\((?<fn>[1-4])\/(?<fd>[1-4])\)))?", RegexOptions.IgnoreCase);
        }

        public HistoricalDate GetDate(string text)
        {
            if (string.IsNullOrEmpty(text) || _yearRegex.IsMatch(text)) return null;

            List<Datation> points = new List<Datation>();
            foreach (Match m in _centRegex.Matches(text))
            {
                Datation d = new Datation
                {
                    Value = int.Parse(m.Groups["v"].Value, CultureInfo.InvariantCulture),
                    IsCentury = true
                };
                if (m.Groups["ie"].Length > 0)
                {
                    // initial: 6 in => c. 510
                    // final: 6 ex => c. 590
                    d.IsCentury = false;
                    d.IsApproximate = true;
                    d.Value = (d.Value - 1) * 100 +
                        (string.Equals(m.Groups["ie"].Value, "in",
                            StringComparison.InvariantCultureIgnoreCase) ? 10 : 90);
                }
                else if (m.Groups["fn"].Length > 0 && m.Groups["fd"].Length > 0)
                {
                    d.IsCentury = false;
                    d.IsApproximate = true;
                    d.Value = (d.Value - 1) * 100;
                    // fraction = 100 / parts
                    int fr = 100 / int.Parse(m.Groups["fd"].Value,
                        CultureInfo.InvariantCulture);
                    // value = mid point between 0 and fraction
                    d.Value += fr * int.Parse(m.Groups["fn"].Value,
                        CultureInfo.InvariantCulture) / 2;
                }

                points.Add(d);
                if (points.Count == 2) break;
            }

            if (points.Count == 0) return null;
            HistoricalDate date = new HistoricalDate();
            if (points.Count == 1) date.SetSinglePoint(points[0]);
            else
            {
                date.SetStartPoint(points[0]);
                date.SetEndPoint(points[1]);
            }
            return date;
        }
    }
}
