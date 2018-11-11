using System;
using System.Net;

namespace GoodsSearcher.Common.Models
{
	public class CustomWebClient : WebClient
	{
		IWebProxy proxy;
		public CustomWebClient(IWebProxy proxy)
		{
			this.proxy = proxy;
		}
		
		protected override WebRequest GetWebRequest(Uri address)
		{
			HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
			request.Proxy = proxy;
			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			return request;
		}
	}
}
