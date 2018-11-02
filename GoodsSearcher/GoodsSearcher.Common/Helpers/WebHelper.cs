using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Text;

namespace Sraper.Common.Models
{
	public static class WebHelper
	{

        public static HtmlDocument GetYahooPageData(string URL)
        {
            if (string.IsNullOrEmpty(URL))
            {
                return null;
            }
            string html = string.Empty;
            html = URL;
            Encoding iso = Encoding.GetEncoding("iso-8859-1");
            HtmlWeb web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = iso,
            };
            HtmlDocument htmlDoc = null;
            try
            {
                htmlDoc = web.Load(html);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return htmlDoc;
        }

        public static HtmlDocument GetPageData(string URL)
		{
			if (string.IsNullOrEmpty(URL))
			{
				return null;
			}
			string html = string.Empty;
			if (!URL.Contains("https://www."))
			{
				html = "https://www." + URL;
			}
			else
			{
				html = URL;
			}
			Encoding iso = Encoding.GetEncoding("iso-8859-1");
			HtmlWeb web = new HtmlWeb()
			{
				AutoDetectEncoding = false,
				OverrideEncoding = iso,
			};
			HtmlDocument htmlDoc = null;
			try
			{
				htmlDoc = web.Load(html);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			return htmlDoc;
		}

        public static HtmlDocument GetShareholderPage(string name, string URL)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            string html = string.Empty;
            if (!URL.Contains("https://www.marketscreener.com"))
            {
                html = "https://www.marketscreener.com" + URL;
            }
            else
            {
                html = URL;
            }
            Encoding iso = Encoding.GetEncoding("iso-8859-1");
            HtmlWeb web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = iso,
            };
            HtmlDocument htmlDoc = null;
            try
            {
                htmlDoc = web.Load(html);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return htmlDoc;
        }
    }
}
