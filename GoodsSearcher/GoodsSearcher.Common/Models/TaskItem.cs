using Flurl.Http;
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
        public FlurlClient proxiedClient = null;

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
                        WebHelper.amazonPageUrl.WithClient(proxiedClient).WithTimeout(2).GetStringAsync().GetAwaiter().GetResult();
                        connectionAccepted = true;
                        workedProxyAddress = correctProxy;
                    }
                    catch (FlurlHttpException ex)
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

        private async void SearchItemOnPage(string combination)
        {
            bool continueWork = true;
            int pageNumber = 1;
            do
            {
                string page = string.Empty;
                var url = WebHelper.CreateUrlToPageResults(combination, pageNumber);
                try
                {
                    page = await url.WithClient(proxiedClient).GetStringAsync();
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
                    var readyItem = item.InitPrice(proxiedClient);
                    if (readyItem != null)
                    {
                        WebHelper.ResultList.Add(readyItem.Result);
                        continueWork = false;
                        break;
                    }
                }
                pageNumber++;
            } while (continueWork);
        }
    }
}
