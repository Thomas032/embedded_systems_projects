using Entra.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Entra.Controller
{
    public class LogController
    {
        private DatabaseController dbController;
        public LogController(DatabaseController databaseController)
        {
            dbController = databaseController;
        }

        public LogModel GetLogModel(SQLiteDataReader rdr)
        {
            LogModel logModel = new LogModel();
            try
            {
                logModel = new LogModel
                {
                    ID = Convert.ToInt32(rdr["id"]),
                    userID = Convert.ToInt32(rdr["userID"]),
                    readerID = Convert.ToInt32(rdr["readerID"]),
                    accessTime = Convert.ToDateTime(rdr["accessTime"]),
                    accessResult = Convert.ToString(rdr["accessResult"])
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
            }

            return logModel;
        }

        public List<LogModel> GetLatestLogs(int count = 5)
        {
            // method that gets the latest logs and returns them as a list of strings
            List<LogModel> logs = new List<LogModel>();
            SQLiteDataReader rdr = dbController.GetModelReader("log", "1", "1");

            if(rdr == null)
            {
                return null;
            }

            int i = 0;

            while (rdr.Read() && i < count)
            {
                LogModel logObj = GetLogModel(rdr);
                logs.Add(logObj);
                i++;
            } 

            return logs;
        }

        public void LogEntry(int userID, int readerID, bool accessResult)
        {
            // method for logging an entry to the database
            DateTime accessTime = DateTime.Now;
            dbController.InsertLog(userID, readerID, accessTime, accessResult);
        }
    }
}
