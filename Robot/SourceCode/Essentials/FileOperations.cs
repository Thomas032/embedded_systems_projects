using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Robot.Essentials
{
    internal class FileOperations
    {
        public const string dirPath = ".\\movements";

        public StreamWriter fileWriter { get; set; }
        public StreamReader fileReader { get; set; }

        public bool InitFile(string FileName)
        {
            // function for establishing connection witht the desired file
            string fullPath = Path.Combine(dirPath, FileName);

            if (File.Exists(fullPath))
            {
                ShowError("Souvour již existuje!");
                return false;
            }

            fileWriter = new StreamWriter(File.Create(fullPath));
            return true;
        }

        public void Write(int mode, char motor, char direction)
        {
            // function for writing movement data to a predefined file
            try
            {
                if (fileWriter == null)
                {
                    InitFile("default.bot");
                }   

                string line = $"{mode}|{motor}|{direction}";
                fileWriter.WriteLine(line);
            }
            catch (Exception ex)
            {
                ShowError($"Chyba při ukládání dat o pohybu do soboru: {ex.Message}");
            }
        }

        public IEnumerable<string[]> ReadFile(string fileName)
        {
            // function for reading a file and yielding the data as it reads it
            string fullPath = Path.Combine(dirPath, fileName);
            string line;

            using (StreamReader fileReader = new StreamReader(fullPath))
            {
                while ((line = fileReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    string[] movementData = line.Split('|');
                    yield return movementData;
                }
            }
        }

        public List<string> GetAvailableFiles()
        {
            // function that returns a list of usable movement files
            List<string> files = new List<string>();
            try
            {
                DirectoryInfo d = new DirectoryInfo(dirPath);
                FileInfo[] Files = d.GetFiles("*.bot");
                foreach (FileInfo file in Files)
                {
                    files.Add(file.Name);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error while getting available files: {ex.Message}");
            }
            return files;
        }
        public void BreakConncetion()
        {
            // function for "breaking" the connection with the file

            // set the file writer to null as the file is already automatically saved
            fileWriter = null;

            // reset the file reader
            fileReader = null;
        }

        public void ShowError(string msg)
        {
            // function for displaying errors by showing a messagebox
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
