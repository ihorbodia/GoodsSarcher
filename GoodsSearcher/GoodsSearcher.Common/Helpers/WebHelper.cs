using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Text;

namespace Sraper.Common.Models
{
	public static class WebHelper
	{
        public static HtmlDocument GetPageData(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				return null;
			}
			string html = string.Empty;
			Encoding iso = Encoding.GetEncoding("iso-8859-1");
			HtmlWeb web = new HtmlWeb()
			{
				AutoDetectEncoding = false,
				OverrideEncoding = iso,
			};
			HtmlDocument htmlDoc = null;
			try
			{
				
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return htmlDoc;
		}

		public static HtmlNode GetSearchResultsTable(string pageContent)
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(pageContent);

			return htmlDocument.DocumentNode
			.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1]");
		}
	}
}
