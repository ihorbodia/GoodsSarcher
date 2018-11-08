using Flurl;
using Flurl.Http;
using GoodsSearcher.Common.Models;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Sraper.Common.Models
{
	public static class WebHelper
	{
        public static string amazonPageUrl = "https://www.amazon.co.uk/";
        public static HtmlNode GetSearchMerchantWordsResultsTable(string pageContent)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(pageContent);

			return htmlDocument.DocumentNode
			.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1]");
		}

        public static string CreateUrlToPageResults(string combination, int pageNumber)
        {
            var items = combination.Split(' ');
            return amazonPageUrl.AppendPathSegment($"s/ref=sr_pg_{pageNumber}")
                      .SetQueryParam("rh", $"i:aps,k:{items[0]} {items[1]} {items[2]}")
                      .SetQueryParam("page", $"{pageNumber}")
                      .AppendPathSegment($"keywords={items[0]} {items[1]} {items[2]}");
        }

        public static IEnumerable<AmazonItem> GetSearchAmazonResults(string pageContent)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContent);

            List<AmazonItem> items = new List<AmazonItem>();
            for (int i = 0; i < 21; i++)
            {
                var document = htmlDocument.DocumentNode.SelectSingleNode($"//*[@id='result_{i}']/div/div/div/div[2]/div[1]/div[1]/a");

                if (document != null)
                {
                    items.Add(new AmazonItem(
                        document.Attributes["href"].Value,
                        htmlDocument.DocumentNode.SelectSingleNode($"//*[@id='result_{i}']").Attributes["data-asin"].Value));
                }
            }
            return items;
        }

        public static FlurlClient CreateProxiedClient(string proxyUrl)
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy(proxyUrl),
                UseProxy = true
            };

            HttpClient client = new HttpClient(handler);
			return new FlurlClient(client).EnableCookies();
        }

		public static FlurlClient CreateClient()
		{
			HttpMessageHandler handler = new HttpClientHandler();
			HttpClient client = new HttpClient(handler);
			return new FlurlClient(client).EnableCookies();
		}
	}
}
