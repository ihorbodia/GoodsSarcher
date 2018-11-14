using HtmlAgilityPack;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace GoodsSearcher.Common.Helpers
{
	public static class DataHelper
	{
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

        public static List<string[]> SplitTitle(string title)
        {
            var result = new List<string[]>();
            var items = title.Split(' ');

            string[] firstGroup = { items[0] ?? "", items[1] ?? "", items[2] ?? "" };
            string[] secondGroup = { items[1] ?? "", items[2] ?? "", items[3] ?? "" };
            string[] thirdGroup = { items[2] ?? "", items[3] ?? "", items[4] ?? "" };

            result.Add(firstGroup);
            result.Add(secondGroup);
            result.Add(thirdGroup);
            return result;
        }
	}
}
