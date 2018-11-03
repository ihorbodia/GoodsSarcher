using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

		public static string SelectFolder()
		{
			FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

			string selectedFolderName = string.Empty;
			if (openFolderDialog.ShowDialog() == DialogResult.OK)
			{
				selectedFolderName = openFolderDialog.SelectedPath;
			}
			else
			{
				selectedFolderName = string.Empty;
			}
			return selectedFolderName;
		}

		public static DataTable ConvertCSVtoDataTable(string strFilePath)
		{
			StreamReader sr = new StreamReader(strFilePath);
			string[] headers = sr.ReadLine().Split(',');
			DataTable dt = new DataTable();
			foreach (string header in headers)
			{
				dt.Columns.Add(header);
			}
			string tab = '\u0009'.ToString();
			string previousRow;
			while (!sr.EndOfStream)
			{
				var currentRow = sr.ReadLine();
				if (currentRow.StartsWith(tab))
				{

				}
				else
				{

				}
				previousRow = sr.ReadLine();
				string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
				DataRow dr = dt.NewRow();
				for (int i = 0; i < headers.Length; i++)
				{
					dr[i] = rows[i];
				}

				dt.Rows.Add(dr);
			}
			return dt;
		}
	}
}
