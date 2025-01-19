using Entra.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Entra.Controller
{
    public class GroupController
    {
        public GroupModel groupObj { get; set; }
        public List<PermissionController> permissions { get; set; }

        private DatabaseController dbController { get; set; }

        public GroupController(DatabaseController databaseController)
        {
            dbController = databaseController;
            permissions = new List<PermissionController>();
        }

        public void AssignModel(SQLiteDataReader rdr)
        {
            try
            {
                groupObj = new GroupModel
                {
                    ID = Convert.ToInt32(rdr["id"]),
                    name = Convert.ToString(rdr["name"]),
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
            }
        }

        public void AssignFromId(int id)
        {
            SQLiteDataReader rdr = dbController.GetModelReader("group", "id", id.ToString());
            // check if no data returned from reder
            if (rdr != null)
            {
                AssignModel(rdr);
                return;
            }

            groupObj = null;
        }

        public void AssignFromName(string name)
        {
            SQLiteDataReader rdr = dbController.GetModelReader("group", "name", name);
            // check if no data returned from reder
            if (rdr != null)
            {
                AssignModel(rdr);
                return;
            }
            groupObj = null;
        }

        public void GetAllPermissions()
        {
            // method that gets all of the permissions and stores them in the permissions list
            permissions.Clear();

            List<int> permissionIDs = dbController.GetGroupPermissionIDs(groupObj.ID);
            foreach (int id in permissionIDs)
            {
                PermissionController permissionController = new PermissionController(dbController);
                permissionController.AssignFromId(id);
                permissions.Add(permissionController);
            }
        }

        public List<string> GetGroups()
        {
            List<string> groups = dbController.GetGroupNames();

            return groups;
        }

        public int CreateNewGroup()
        {
            // method for creating a new group
            int status = dbController.CreateNewGroup(groupObj.name, permissions);
            return status;

        }

        public void UpdateGroup()
        {
            // method for updating a group
            dbController.UpdateGroup(groupObj.ID, groupObj.name, permissions);
        }

        public void DeleteGroup()
        {
            // method for deleting a group
            dbController.DeleteGroup(groupObj.ID);
        }
    }
}
