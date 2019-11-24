using System;
using System.IO;

namespace Models
{
    public static class Debug
    {
        static string myPath = System.AppDomain.CurrentDomain.BaseDirectory + @"Log";
        static string fileName = @"\log.txt";

        public static void Text(Exception exception, string info = "")
        {
            try
            {
                // create folder
                if (!Directory.Exists(myPath)) Directory.CreateDirectory(myPath);

                // create file
                if (!File.Exists(myPath + fileName)) File.Create(myPath + fileName);

                File.AppendAllText(myPath + fileName, DateTime.Now.ToString() + " --> info: " + info + Environment.NewLine + exception.Message + Environment.NewLine + "______________________________________" + Environment.NewLine);
            }
            catch { }
        }
    }
}
