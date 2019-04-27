using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CopyPictures
{
    class Program
    {
        private static Regex r = new Regex(":");
        private static string _source = "";
        private static string _dest = "";

        static void Main(string[] args)
        {
            string a = "branch_#7";
            _source = ConfigurationManager.AppSettings["sourceFolder"];
            _dest = ConfigurationManager.AppSettings["destinationFolder"];
            if (args.Length > 0)
                _source = args[0];

            foreach (string file in Directory.GetFiles(_source))
            { 
                FileInfo fileInfo = new FileInfo(file);
                DateTime dateTaken = GetDateTakenFromImage(file);
                if (dateTaken.Equals(DateTime.MinValue))
                {
                    dateTaken = fileInfo.LastWriteTime;
                } 

                 string fileName = fileInfo.Name;
                int errorCode = -1;
                bool isSuccess = CopyFile(fileInfo, dateTaken, out errorCode);
                if (isSuccess)
                {
                    File.Move(file, file.Replace(fileName, "copy_" + fileName));
                }
                else
                { 
                    if (errorCode == 1)
                        File.Move(file, file.Replace(fileName, "exist_" + fileName));
                }
            }
        }
        
        public static DateTime GetDateTakenFromImage(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (Image myImage = Image.FromStream(fs, false, false))
                    {
                        PropertyItem propItem = myImage.GetPropertyItem(36867);
                        string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                        return DateTime.Parse(dateTaken);
                    }
                }
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;
            }
        }

        public static bool CopyFile(FileInfo fileInfo, DateTime dateTaken, out int errorCode)
        {
            errorCode = -1;
            try
            {
                string directory = string.Format(@"{0}\{1}\{2}", _dest, dateTaken.Year, dateTaken.ToString("MMddyyyy"));
                Directory.CreateDirectory(directory);

                File.Copy(fileInfo.FullName, string.Format(@"{0}\{1}", directory, fileInfo.Name));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error copying file: {0} - {1}", fileInfo.Name, ex.Message));
                if (ex.Message.ToLower().Contains("exist"))
                    errorCode = 1;
                return false;
            }
        }
    }
}
