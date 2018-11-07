﻿using HtmlAgilityPack;
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

		public static IEnumerable<string> GetHrefsFromHtmlList(HtmlNode table)
		{
			if (table == null)
			{
				return null;
			}
			var list = table.ChildNodes
				.Select(x => x.Descendants("h3").FirstOrDefault())
				.Where(x => x != null)
				.SelectMany(x => x.ChildNodes)
				.Where(x => x.Name.Equals("a"))
				.Select(y => y.Attributes["href"].Value);
			return list;
		}
	}
}
