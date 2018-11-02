using System.Windows.Forms;

namespace Sraper.Common
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
    }
}
