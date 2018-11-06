using Flurl.Http;
using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Sraper.Common.Models
{
	public static class WebHelper
	{
		public static HtmlNode GetSearchResultsTable(string pageContent)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(pageContent);

			return htmlDocument.DocumentNode
			.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1]");
		}

        public static IFlurlClient CreateProxiedClient(string proxyUrl)
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy(proxyUrl),
                UseProxy = true
            };

            HttpClient client = new HttpClient(handler);

            return new FlurlClient(client);
        }
    }
}
