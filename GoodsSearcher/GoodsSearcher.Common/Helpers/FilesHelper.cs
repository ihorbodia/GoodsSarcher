using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GoodsSearcher.Common.Helpers
{
	public static class FilesHelper
	{
		public static string SelectFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			string selectedFileName = string.Empty;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				selectedFileName = openFileDialog.FileName;
			}
			else
			{
				selectedFileName = string.Empty;
			}
			return selectedFileName;
		}

		public static List<string> ConvertCSVtoListofTitles(string strFilePath)
		{
			StreamReader sr = new StreamReader(strFilePath);
			string[] headers = sr.ReadLine().Split(',');
			DataTable dt = new DataTable();
			foreach (string header in headers)
			{
				dt.Columns.Add(header);
			}
			string tab = '\u0009'.ToString();
			string previousRow = null;
			while (!sr.EndOfStream)
			{
				var currentRow = sr.ReadLine();
				if (currentRow.StartsWith(tab) && !string.IsNullOrEmpty(previousRow))
				{
					currentRow = currentRow.Replace(tab, " ");
					currentRow = previousRow + currentRow;
				}
				string[] rows = Regex.Split(currentRow, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
				if (rows.Count() == 1)
				{
					previousRow = currentRow;
					continue;
				}
				DataRow dr = dt.NewRow();
				for (int i = 0; i < headers.Length; i++)
				{
					dr[i] = rows[i].TrimStart('"').TrimEnd('"');
				}
				
				dt.Rows.Add(dr);
			}
            sr.Dispose();
            return dt.AsEnumerable()
                .Select(s => s.Field<string>("eBay Title")
                .Replace('\uFFFD'.ToString(), "")
                .Replace("[", "")
                .Replace("]", ""))
                .ToList(); 
		}

        public static Dictionary<string, bool> ConvertProxyFileToDictionary(string pathToProxyFile)
        {
            var logFile = File.ReadAllLines(pathToProxyFile);
            return logFile.ToDictionary(key => key, value => true);
        }
	}
}
