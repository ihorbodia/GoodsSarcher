using HtmlAgilityPack;
using System.Data;
using System.Linq;
namespace GoodsSearcher.Common.Helpers
{
	public class DataHelper
	{
		public static DataTable ConvertHtmlTableToDataTable(HtmlNode table)
		{
			var headers = table.SelectNodes("//tr/th");
			DataTable dataTable = new DataTable();

			foreach (HtmlNode header in headers)
				dataTable.Columns.Add(header.InnerText);

			foreach (var row in table.SelectNodes("//tr[td]"))
				dataTable.Rows.Add(row.SelectNodes("td").Select(td => td.InnerText).ToArray());

			return dataTable;
		}
	}
}
