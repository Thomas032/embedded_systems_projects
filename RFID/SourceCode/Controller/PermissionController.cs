using Entra.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Entra.Controller
{
    public class PermissionController
    {
        public PermissionModel permissionObj { get; set; }
        private DatabaseController dbController;

        public PermissionController(DatabaseController databaseController)
        {
            dbController = databaseController;
        }

        public void AssignModel(SQLiteDataReader rdr)
        {
            try
            {
                permissionObj = new PermissionModel
                {
                    ID = Convert.ToInt32(rdr["id"]),
                    startDay = Convert.ToInt32(rdr["startDay"]),
                    endDay = Convert.ToInt32(rdr["endDay"]),
                    startTime = Convert.ToString(rdr["startTime"]),
                    endTime = Convert.ToString(rdr["endTime"]),
                    readerID = Convert.ToInt32(rdr["readerID"])
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
            }
        }

        public string GetString()
        {
            return $"{GetReaderName()} [{permissionObj.startDay}...{permissionObj.endDay}] {permissionObj.startTime}-{permissionObj.endTime}";
        }

        public void AssignFromId(int id)
        {
            // function that asigns the model from a given id
            SQLiteDataReader rdr = dbController.GetModelReader("permission", "id", id.ToString());
            AssignModel(rdr);
        }

        public bool AccessGranted(int day, DateTime time, int reader)
        {
            // method for checking if certain permission is grated at a given time, day and place

            if (permissionObj.startDay <= day && permissionObj.endDay >= day)
            {
                DateTime now = DateTime.Parse($"{time.Hour}:{time.Minute}");
                DateTime start = DateTime.Parse(permissionObj.startTime);
                DateTime end = DateTime.Parse(permissionObj.endTime);

                if (start <= now && now <= end)
                {
                    if (permissionObj.readerID - 1 == reader)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<PermissionController> GetAllPermissions()
        {
            // method for getting all permissions from the database
            List<PermissionModel> permissions = dbController.GetAllPermissions();
            List<PermissionController> permissionControllers = new List<PermissionController>();

            foreach (PermissionModel permission in permissions)
            {
                PermissionController permissionController = new PermissionController(dbController);
                permissionController.permissionObj = permission;
                permissionControllers.Add(permissionController);
            }
            
            return permissionControllers;
        }

        public string GetReaderName()
        {
            // method for getting the name of the reader from the database
            return dbController.GetReaderName(permissionObj.readerID);
        }

        public void CreateNewPermission(int startDay, int endDay, string startTime, string endTime, int readerID)
        {
            // method for creating a new permission
            dbController.InsertNewPermission(startDay, endDay, startTime, endTime, readerID);
        }

        public void DeletePermission(int id)
        {
            // method for deleting a permission
            dbController.DeletePermission(id);
        }

        public PermissionController GetPermissionByString(string permissionString)
        {
            // method for getting a permission by a given string
            List<PermissionModel> permissions = dbController.GetAllPermissions();
            foreach (PermissionModel permission in permissions)
            {
                PermissionController permissionController = new PermissionController(dbController);
                permissionController.permissionObj = permission;
                if (permissionController.GetString() == permissionString)
                {
                    return permissionController;
                }
            }
            return null;
        }

        public void UpdatePermission(int id, int startDay, int endDay, string startTime, string endTime, int readerID)
        {
            // method for updating a permission
            dbController.UpdatePermission(id, startDay, endDay, startTime, endTime, readerID);
        }
    }
}
