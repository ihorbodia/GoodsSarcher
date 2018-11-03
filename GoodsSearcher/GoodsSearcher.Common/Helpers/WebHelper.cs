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
    }
}
