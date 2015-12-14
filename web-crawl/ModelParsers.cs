using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulpine.Model;
using HtmlAgilityPack;

namespace DataExtraction
{
    public static class ModelParsers
    {
        public static IEnumerable<CardAuthorization> AsCardAuthorizations(this IEnumerable<string> pages)
        {
            foreach (var page in pages)
            {
                foreach (var item in _GetAuthorizationsFromPayload(page))
                {
                    yield return item;
                }
            }
        }

        private static List<CardAuthorization> _GetAuthorizationsFromPayload(string payload)
        {
            string rawPayload = payload;

            rawPayload = ExtractValuablePayload(rawPayload);

            //Get full card numbers
            int fullCardExtractStartAt = rawPayload.LastIndexOf("value='[".Replace("'", "\""));
            int fullCardExtractEndAt = rawPayload.LastIndexOf("]' />".Replace("'", "\""));
            string rawFullCardNumbers = rawPayload.Substring(fullCardExtractStartAt, fullCardExtractEndAt - fullCardExtractStartAt)
                .Replace("value=\"[&#39;", "")
                .Replace("&#39;", "");

            List<string> fullCardNumbers = rawFullCardNumbers.Split(',').Distinct().ToList();

            Dictionary<string, string> cardNumbers = new Dictionary<string, string>();
            fullCardNumbers.ForEach(c => cardNumbers.Add(c.Substring(6, 6), c));

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(rawPayload);

            var rawAuthorizationRows = document.DocumentNode.Elements("tr");

            var result = new List<CardAuthorization>();
            foreach (var row in rawAuthorizationRows)
            {
                var rawColumns = row.Elements("td");

                if (Cast.ParseLong(rawColumns.ElementAt(1).InnerHtml) == 0
                    && Cast.ParseLong(rawColumns.ElementAt(4).InnerHtml) == 0) continue;

                var item = new CardAuthorization();
                item.ShortCardNumber = Cast.ParseLong(rawColumns.ElementAt(1).InnerHtml);
                item.CardNumber = Cast.ParseLong(cardNumbers[rawColumns.ElementAt(1).InnerHtml]);
                item.AccountNumber = Cast.ParseLong(rawColumns.ElementAt(4).InnerHtml);
                item.InaAuthorizationNumber = Cast.ParseLong(rawColumns.ElementAt(2).InnerHtml);

                var numberFormater = System.Globalization.CultureInfo.GetCultureInfo("hr").NumberFormat;
                var calendar = System.Globalization.CultureInfo.GetCultureInfo("hr").Calendar;

                var dateFragments = rawColumns.ElementAt(3).InnerHtml.Split(new char[] { '.' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(fragment => Convert.ToInt32(fragment)).ToArray();
                var timeFragments = rawColumns.ElementAt(5).InnerHtml.Split(new char[] { '.' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(fragment => Convert.ToInt32(fragment)).ToArray();
                item.AuthorizedOn = (new DateTime(dateFragments[2], dateFragments[1], dateFragments[0], timeFragments[0], timeFragments[1], timeFragments[2], calendar)).ToUniversalTime();

                //item.AmountInKuna = Cast.ParseDouble(rawColumns.ElementAt(6).InnerHtml);
                item.AmountInKuna = Convert.ToDouble(rawColumns.ElementAt(6).InnerHtml, numberFormater);

                result.Add(item);
            }

            return result;
        }
        private static string ExtractValuablePayload(string rawPayload)
        {
            int firstResultIndex = rawPayload.IndexOf("'result':'");
            int lastResultIndex = rawPayload.LastIndexOf("','id':0})");

            rawPayload = rawPayload.Substring(firstResultIndex, lastResultIndex - firstResultIndex).Replace("'result':'", "");
           
            string partialElementId = "dxo.InlineInitialize();";
            var firstIndex = rawPayload.LastIndexOf(partialElementId);
            var lastIndex = rawPayload.LastIndexOf("</tr><tr class='dxgvDataRow'>".Replace("'", "\""));
            rawPayload = rawPayload.Substring(firstIndex, lastIndex - firstIndex);

            var additionalIndex = rawPayload.IndexOf("<td class='dxgv'>".Replace("'", "\""));
            rawPayload = rawPayload.Substring(additionalIndex);
            return string.Concat(rawPayload);
        }
    }
}
