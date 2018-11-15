using GoodsSearcher.Common.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
namespace GoodsSearcher.Common.Helpers
{
	public static class DataHelper
	{
        public static StreamWriter sw;
        static readonly object lockObject = new object();

        public static void WriteDataToFile(AmazonItem item)
        {
            lock(lockObject)
            {
                sw.WriteLine($"{item.Combination},{item.ASIN},{item.Price}{Environment.NewLine}");
            }
        }

        public static DataTable ConvertHtmlTableToDataTable(HtmlNode table)
		{
			if (table == null)
			{
				return null;
			}
			var headers = table.SelectNodes("//tr/th");
			DataTable dataTable = new DataTable();

			foreach (HtmlNode header in headers)
				dataTable.Columns.Add(header.InnerText.Trim());

			foreach (var row in table.SelectNodes("//tr[td]"))
				dataTable.Rows.Add(row.SelectNodes("td").Select(td => td.InnerText.Trim().ToLower()).ToArray());

			return dataTable;
		}

		public static int ToInt(this string s)
		{
			int i;
			s = string.IsNullOrEmpty(s) ? string.Empty : s.Replace(",", "");
			if (int.TryParse(s, out i))
			{
				return i;
			}
			return 0;
		}

        public static KeyValuePair<string, int> GetMaximumCombinationFromDict(Dictionary<string, int> combinations)
        {
            KeyValuePair<string, int> maxValue = new KeyValuePair<string, int>(string.Empty, 0);
            foreach (var item in combinations)
            {
                if (item.Value > maxValue.Value)
                {
                    maxValue = item;
                }
            }
            return maxValue;
        }

        public static List<string[]> SplitTitle(string title, bool isMoreThanThreeParts)
        {
            var result = new List<string[]>();
            int firstWordIndex = 1;
            int secondWordIndex = 2;
            int thirdWordIndex = 3;
            if (!isMoreThanThreeParts)
            {
                var items = title.Split(' ');
                while (thirdWordIndex < items.Length)
                {
                    string[] combination = { items[firstWordIndex] ?? "", items[secondWordIndex] ?? "", items[thirdWordIndex] ?? "" };
                    if (combination.Length > 0)
                    {
                        result.Add(combination);
                        if (result.Count == 3)
                        {
                            break;
                        }
                        firstWordIndex++;
                        secondWordIndex++;
                        thirdWordIndex++;
                    }
                }
            }
            else
            {
                var items = title.Split(' ');
                while (thirdWordIndex < items.Length)
                {
                    string[] combination = { items[firstWordIndex] ?? "", items[secondWordIndex] ?? "", items[thirdWordIndex] ?? "" };
                    if (combination.Length > 0)
                    {
                        result.Add(combination);
                        firstWordIndex++;
                        secondWordIndex++;
                        thirdWordIndex++;
                    }
                }
            }
            
            return result;
        }
	}
}
