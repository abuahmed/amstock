using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMStock.SyncEngine
{
    public class ServiceLog
    {
        public static void Log(string txt)
        {
            try
            {
                var todayShort = DateTime.Now.ToShortDateString();

                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\AMStockSyncLog\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var fileLoc = Path.Combine(path, todayShort.Replace("/", "_") + ".amstocklog");
                
                if (!File.Exists(fileLoc))
                {
                    File.Create(fileLoc).Close();
                }

                byte[] byteData = null;
                byteData = Encoding.ASCII.GetBytes("\r\n" + DateTime.Now.ToLongTimeString() + txt);
                var wFile = new FileStream(fileLoc, FileMode.Append, FileAccess.Write);
                wFile.Write(byteData, 0, byteData.Length);
                wFile.Close();
            }
            catch
            {

            }

        }

        //public static void Log(string logMessage, TextWriter w)
        //{
        //    w.Write("\r\nLog Entry : ");
        //    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
        //        DateTime.Now.ToLongDateString());
        //    w.WriteLine("  :");
        //    w.WriteLine("  :{0}", logMessage);
        //    w.WriteLine("-------------------------------");
        //}

        //public static void DumpLog(StreamReader r)
        //{
        //    string line;
        //    while ((line = r.ReadLine()) != null)
        //    {
        //        Console.WriteLine(line);
        //    }
        //}
    }
}
