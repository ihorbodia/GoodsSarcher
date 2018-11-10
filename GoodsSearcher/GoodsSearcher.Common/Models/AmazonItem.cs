using Flurl.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace GoodsSearcher.Common.Models
{
    public class AmazonItem
    {
        public string Href { get; private set; }
        public string Price { get; private set; }
        public string ASIN { get; private set; }
        public string Combination { get; private set; }

        public AmazonItem(string href, string ASIN, string combination)
        {
            Href = href;
            this.ASIN = ASIN;
            Combination = combination;
        }

       
        public async Task<AmazonItem> InitPrice(FlurlClient client)
        {
            if (string.IsNullOrEmpty(Href))
            {
                return null;
            }

            var html = await Href.WithClient(client).GetStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var tag = document.GetElementbyId("merchant-info");
            if (tag == null)
            {
                return null;
            }

            HtmlNode price = document.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']");
            if (price == null)
            {
                price = document.DocumentNode.SelectSingleNode("//*[@id='buyNewSection']/div/div/div/div[2]/a/div/div[2]/span/span");
            }
            if (price == null)
            {
                price = document.DocumentNode.SelectSingleNode("//*[@id='buyNewSection']/div/div/span/span");
            }
            Price = price.InnerText;
            return this;
        }
    }
}
