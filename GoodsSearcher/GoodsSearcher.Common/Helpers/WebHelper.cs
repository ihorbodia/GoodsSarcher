using Flurl;
using Flurl.Http;
using GoodsSearcher.Common.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Sraper.Common.Models
{
	public static class WebHelper
	{

        public static string amazonPageUrl = "https://www.amazon.co.uk/";
        static readonly Random rnd = new Random();
        public static ConcurrentDictionary<string, int> Proxies;
        public static ConcurrentBag<AmazonItem> ResultList = new ConcurrentBag<AmazonItem>();
        public static HtmlNode GetSearchMerchantWordsResultsTable(string pageContent)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(pageContent);

			return htmlDocument.DocumentNode
			.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1]");
		}

        public static string GetRandomProxyAddress()
        {
            var itemsToRemove = Proxies.Where(x => x.Value > 2);
            foreach (var item in itemsToRemove)
            {
                Proxies.TryRemove(item.Key, out int value);
            }
            int r = rnd.Next(Proxies.Count);
            return Proxies.ElementAt(r).Key;
        }

        public static string CreateUrlToPageResults(string combination, int pageNumber)
        {
            var items = combination.Split(' ');
            return amazonPageUrl.AppendPathSegment($"s/ref=sr_pg_{pageNumber}")
                      .SetQueryParam("rh", $"i:aps,k:{items[0]} {items[1]} {items[2]}")
                      .SetQueryParam("page", $"{pageNumber}")
                      .SetQueryParam("ie", "UTF8")
                      .AppendPathSegment($"keywords={items[0]} {items[1]} {items[2]}");
        }

        public static IEnumerable<AmazonItem> GetSearchAmazonResults(string pageContent, string combination)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContent);

            List<AmazonItem> items = new List<AmazonItem>();
            for (int i = 0; i < 21; i++)
            {
                var document = htmlDocument.DocumentNode.SelectSingleNode($"//*[@id='result_{i}']/div/div/div/div[2]/div[1]/div[1]/a");
				
				if (document != null)
                {
					var asin = htmlDocument.DocumentNode.SelectSingleNode($"//*[@id='result_{i}']").Attributes["data-asin"].Value;
					var price = htmlDocument.DocumentNode.SelectSingleNode($"//*[@id='result_{i}']/div/div/div/div[2]/div[2]/div[1]/div[1]/a/span[2]")?.InnerText;
					if (!string.IsNullOrEmpty(price) && price.Contains("-"))
					{
						price = null;
					}
					items.Add(new AmazonItem(
                        document.Attributes["href"].Value,
						asin,
						combination,
						price));
                }
            }
            return items;
        }

        public static CustomWebClient CreateProxiedClient(string proxyUrl)
        {
			return new CustomWebClient(new WebProxy(proxyUrl));
        }

		public static FlurlClient CreateClient()
		{
			HttpMessageHandler handler = new HttpClientHandler();
			HttpClient client = new HttpClient(handler);
			return new FlurlClient(client).EnableCookies();
		}
	}
}
