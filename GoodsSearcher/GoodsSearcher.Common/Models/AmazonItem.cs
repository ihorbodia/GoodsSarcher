using HtmlAgilityPack;

namespace GoodsSearcher.Common.Models
{
    public class AmazonItem
    {
        public string Href { get; private set; }
        public string Price { get; private set; }
        public string ASIN { get; private set; }
        public string Combination { get; private set; }

        public AmazonItem(string href, string ASIN, string combination, string price = "")
        {
            Href = href;
            this.ASIN = ASIN;
            Combination = combination;
			Price = price;
        }

		public void ClearPrice()
		{
			if (string.IsNullOrEmpty(Price))
			{
				return;
			}
			string latinCapLetter = '\u00C2'.ToString();
			string poundSymbol = '\u00A3'.ToString();
			Price = Price.Replace(latinCapLetter, "").Replace(poundSymbol, "");
		}

        public bool IsProductDispatched(CustomWebClient client)
        {
            if (string.IsNullOrEmpty(Href))
            {
                return false;
            }
            var html = client.DownloadString(Href);
            if (html.Contains("Dispatched from and sold by Amazon"))
            {
                return true;
            }
            return false;
        }

        public AmazonItem InitPrice(CustomWebClient client)
		{
			if (string.IsNullOrEmpty(Href))
			{
				return null;
			}
			var html = client.DownloadString(Href);
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(html);
			var tag = document.GetElementbyId("merchant-info");
			if (tag == null)
			{
				return null;
			}

			HtmlNode price = document.DocumentNode.SelectSingleNode("//span[@id=\"priceblock_ourprice\"]");
			var items = document.DocumentNode.SelectSingleNode("//*[@id='price']");
			if (price == null)
			{
				price = document.DocumentNode.SelectSingleNode("//*[@id='buyNewSection']/div/div/div/div[2]/a/div/div[2]/span/span");
			}
			if (price == null)
			{
				price = document.DocumentNode.SelectSingleNode("//*[@id='buyNewSection']/div/div/span/span");
			}
			if (price == null)
			{
				price = document.DocumentNode.SelectSingleNode("//*[@id='soldByThirdParty']/span[1]");
			}
			if (price == null)
			{
				price = document.DocumentNode.SelectSingleNode("//*[@id='oneTimeBuyBox']/div/div[1]/a/h5/div/div[2]/span");
			}
			if (price == null)
			{
				price = document.DocumentNode.SelectSingleNode("//*[@id=\"priceblock_ourprice\"]");
			}
			if (price == null)
			{
				Price = "0";
			}
			else
			{
				Price = price.InnerText;
			}
			return this;
		}
	}
}
