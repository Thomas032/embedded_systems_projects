using Entra.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entra.Controller
{
    public class UserController
    {
        public UserModel userObj { get; set; }

        private DatabaseController dbController;
        public GroupController groupController;

        public UserController (DatabaseController databaseController)
        {
            dbController = databaseController;
            userObj = new UserModel();
            groupController = new GroupController(databaseController);
        }

        public void AssignGroup()
        {
            // method that assigns the group to the user
            //Console.WriteLine(userObj.groupID.ToString()) ;
            SQLiteDataReader rdr = dbController.GetGroup(userObj.groupID);

            if(rdr == null)
            { 
                return;
            }

            groupController.AssignModel(rdr);

        }

        public string GetGroupName()
        {
            // simple method for getting the name of the group
            return groupController.groupObj.name;
        }

        public bool HasAccess(int readerID)
        {
            // method that iterates through the permissions of the group and checks if the user has access to the given reader
            groupController.GetAllPermissions();

            foreach(PermissionController permission in groupController.permissions)
            {
                if(permission.AccessGranted((int)DateTime.Now.DayOfWeek, DateTime.Now, readerID))
                {
                    return true;
                }
            }
            return false;
        }

        public void AssignModel(SQLiteDataReader reader)
        {
            // method that assigns the model from the reader
            try
            {
                userObj = new UserModel
                {
                    ID = Convert.ToInt32(reader["id"]),
                    name = Convert.ToString(reader["name"]),
                    surname = Convert.ToString(reader["surname"]),
                    code = Convert.ToString(reader["code"]),
                    groupID = Convert.ToInt32(reader["groupID"]),
                };
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error occured: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AssignFromCode(string code)
        {
            // method that assigns the model from the access code he uses
            SQLiteDataReader rdr = dbController.GetUserByCode(code); // select all columns from user table

            if(rdr == null)
            {
                // user with the specified code does not exist
                userObj = null;
                return;
            }

            string userCode = Convert.ToString(rdr["code"]);

            if(code == userCode)
            {
                AssignModel(rdr);
                return;
            }

            // code not found -> set user to null
            userObj = null;
        }

        public void AssignFromName(string name, string surname)
        {
            // method that assigns the model from the name and surname
            SQLiteDataReader rdr = dbController.GetUserByName(name, surname); // select all columns from user table

            if (rdr == null)
            {
                userObj = null;
                return;
            }

            AssignModel(rdr);
        }

        public void CreateUser()
        {
            // method that creates a new user
            dbController.InsertUser(userObj.name, userObj.surname, userObj.code ,userObj.groupID);
        }

        public void UpdateUser(string code, int groupID)
        {
            // method that updates the user
            dbController.UpdateUser(userObj.ID, userObj.name, userObj.surname, code, groupID);
        }

        public void DeleteUser()
        {
            // method that deletes the user
            dbController.DeleteUser(userObj.ID);
        }
    }
}
