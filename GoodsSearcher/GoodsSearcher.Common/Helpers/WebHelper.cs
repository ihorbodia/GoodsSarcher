using Flurl.Http;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Sraper.Common.Models
{
	public static class WebHelper
	{
		public static HtmlNode GetSearchMerchantWordsResultsTable(string pageContent)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(pageContent);

			return htmlDocument.DocumentNode
			.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1]");
		}

		public static HtmlNode GetSearchEbayResultsTable(string pageContent)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(pageContent);
			var document = htmlDocument.GetElementbyId("ListViewInner"); //.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1])"
			var items = new List<HtmlNode>(document.ChildNodes.Where(x => !x.Name.Contains("li")));
			foreach (var item in items)
			{
				document.RemoveChild(item);
			}
			return document;
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
