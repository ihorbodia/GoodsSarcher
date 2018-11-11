using Sraper.Common.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GoodsSearcher.Common.Models
{
	public class TaskItem
    {
        public Task Task { get; set; }
        public string workedProxyAddress;
        public CustomWebClient proxiedClient = null;

        public Task StartTask(string combination)
        {
            Task = Task.Factory.StartNew(new Action<object>((workedProxy) =>
            {
                string workedProxyString = string.Empty;
                if (workedProxy != null)
                {
                    workedProxyString = workedProxy.ToString();
                }
                string correctProxy = string.Empty;
                bool connectionAccepted = false;
                do
                {
                    if (string.IsNullOrEmpty(workedProxyString))
                    {
                        correctProxy = WebHelper.GetRandomProxyAddress();
                        proxiedClient = WebHelper.CreateProxiedClient(correctProxy);
                    }
                    else
                    {
                        correctProxy = workedProxyString;
                    }
                    if (string.IsNullOrEmpty(correctProxy))
                    {
                        Debug.WriteLine("Return because proxy is null");
                        return;
                    }
                    try
                    {
						proxiedClient.DownloadData(WebHelper.amazonPageUrl);
						connectionAccepted = true;
                        workedProxyAddress = correctProxy;
                    }
                    catch (Exception ex)
                    {
                        WebHelper.Proxies.TryRemove(correctProxy, out int value);
                        workedProxyAddress = string.Empty;
                        workedProxyString = string.Empty;
                        Debug.WriteLine(WebHelper.Proxies.Count);
                    }
                } while (!connectionAccepted);
                SearchItemOnPage(combination);
            }), workedProxyAddress);
            return Task;
        }

        private void SearchItemOnPage(string combination)
        {
            bool continueWork = true;
            int pageNumber = 1;
            do
            {
                string page = string.Empty;
                var url = WebHelper.CreateUrlToPageResults(combination, pageNumber);
				try
                {
					page = proxiedClient.DownloadString(url);
				}
                catch (Exception ex)
                {
                    break;
                }
                var itemsOnPage = WebHelper.GetSearchAmazonResults(page, combination);
                if (itemsOnPage.ToList().Count == 0)
                {
                    break;
                }
                foreach (var item in itemsOnPage)
                {
					var readyItem = item;
                    if (readyItem != null)
                    {
						if (string.IsNullOrEmpty(readyItem.Price))
						{
							readyItem.InitPrice(proxiedClient);
						}
						readyItem.ClearPrice();
						WebHelper.ResultList.Add(readyItem);
                        continueWork = false;
                        break;
                    }
                }
                pageNumber++;
            } while (continueWork);
        }
    }
}
