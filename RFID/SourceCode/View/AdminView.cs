using Entra.Controller;
using Entra.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Entra.View
{
    public partial class AdminView : Form
    {
        public System.Timers.Timer pollTimer { get; set; }
        public DatabaseController databaseController { get; set; }
        public UserController userController { get; set; }
        public GroupController groupController { get; set; }
        public SerialController serialController { get; set; }
        public ReaderController readerController { get; set; }
        public PermissionController permissionController { get; set; }
        public List<PermissionController> currentPermissions { get; set; }


        public string code { get; set; }
        internal int lastReadStatus { get; set; }

        public AdminView()
        {
            InitializeComponent();
        }

        private void AdminView_Load(object sender, EventArgs e)
        {
            // assign the global components
            databaseController = new DatabaseController();
            userController = new UserController(databaseController);
            groupController = new GroupController(databaseController);
            readerController = new ReaderController(databaseController);
            permissionController = new PermissionController(databaseController);
            currentPermissions = new List<PermissionController>();

            // initialize the serial controller
            serialController = new SerialController("COM13", 19200);
            serialController.Init();

            // global variabled
            lastReadStatus = 0;
            code = "";

            //setup the poll timer
            pollTimer = new System.Timers.Timer();
            pollTimer.Interval = 200;
            pollTimer.Elapsed += pollTimerElapsed;
            pollTimer.Start();

            // set the listbox to be able to select multiple items
            group_permissions.SelectionMode = SelectionMode.MultiExtended;
    
            // pupulate table with data
            PopulateGroups(user_group_combo);
            PopulateGroups(group_group_combo);
            PopulatePermissionsList();
            PopulatePeermissionsCombo();
            PopulateReaders();

        }

        private void pollTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // method for checking if any code has been entered
            // the sole puprose of this method is to auto-fill the code input to search user

            if (lastReadStatus != serialController.readCounter)
            {
                // get the scanned code
                code = serialController.currentCode;

                userController.AssignFromCode(code);

                // check if the model was occupied with data
                if(userController.userObj != null)
                {
                    // user exists -> populate the current user group
                    groupController.AssignFromId(userController.userObj.groupID);
                    // call function to show the found user
                    ShowUserData(true);
                }
                // if the model has data -> show it
                // else display error message
                lastReadStatus = serialController.readCounter;
            }
        }

        private void ShowUserData(bool dataFromReader = false)
        {
            name_input.Text = userController.userObj.name;
            surname_input.Text = userController.userObj.surname;
            
            if(dataFromReader)
            {
                code_input.Text = code.ToString();
            }
            else
            {
                code_input.Text = userController.userObj.code.ToString();
            }

            // populate the group comboboxes
            PopulateGroups(user_group_combo);
            PopulateGroups(group_group_combo);

            Console.Error.WriteLine("User group name = " + groupController.groupObj.name);
            
            
            // find out the index of the specific user group
            int selectedIndex = user_group_combo.FindString(groupController.groupObj.name);
            
            // select the group item
            user_group_combo.SelectedIndex = selectedIndex;
            
        }

        private void PopulateGroups(ComboBox box)
        {
            // method for populating the group combobox
            // get the list of groups
            currentPermissions.Clear();

            List<string> groups = groupController.GetGroups();

            box.Items.Clear();

            foreach(string group in groups)
            {
                box.Items.Add(group);
            }

        }

        private void PopulatePermissionsList()
        {
            // method for populating the permissions list

            // get the list of permissions
            currentPermissions.Clear();
            List<PermissionController> permissions = permissionController.GetAllPermissions();

            if(permissions == null)
            {
                // we have no permissions -> return
                NotifyUser("No permissions in db");
                return;
            }

            // cler the listbox
            permission_combo.Items.Clear();

            foreach(PermissionController permission in permissions)
            {
                permission_combo.Items.Add(permission.GetString());
                currentPermissions.Add(permission);
            }
        }

        private void PopulatePeermissionsCombo()
        {
            // method for populating the permissions combobox

            // get the list of permissions
            List<PermissionController> permissions = permissionController.GetAllPermissions();

            if (permissions == null)
            {
                // we have no permissions -> return
                NotifyUser("No permissions in db");
                return;
            }

            // cler the listbox
            group_permissions.Items.Clear();

            foreach (PermissionController permission in permissions)
            {
                group_permissions.Items.Add(permission.GetString());
                currentPermissions.Add(permission);
            }
        }

        public void PopulateReaders()
        {
            // method that fills in the data of a textbox with the reader names
            reader_combo.Items.Clear();

            List<string> readers = readerController.GetReaderNames();

            foreach(string reader in readers)
            {
                reader_combo.Items.Add(reader);
            }
        }

        private void user_find_button_Click(object sender, EventArgs e)
        {
            // reset the data
            ResetGlobalObjects();

            // user clicked the find button
            // check if required fields are set
            if(name_input.Text == "" || surname_input.Text == "")
            {
                NotifyUser("Please fill in the name and surname before searching.");
                return;
            }

            // try to assign the usercontroller with data
            userController.AssignFromName(name_input.Text, surname_input.Text);

            // check if data assigned -> user exists 
            if(userController.userObj == null)
            {
                NotifyUser("User not found.");
                ResetData();
                return;
            }

            // user exists -> populate the current user group
            groupController.AssignFromId(userController.userObj.groupID);

            if(groupController.groupObj == null)
            {
                NotifyUser("User has no assigned groups;");
            }

            // call function to show the found user
            ShowUserData(false);
        }

        private void NotifyUser(string message)
        {
            // message for displaying user related messages
            MessageBox.Show(message);
        }

        private void ResetGlobalObjects()
        {
            // method for resetting the data
            userController.userObj = null;
            groupController.groupObj = null;
        }

        private void ResetData()
        {
            // method for resetting the data
            name_input.Text = "";
            surname_input.Text = "";
            code_input.Text = "";
            user_group_combo.SelectedIndex = -1;
            group_group_combo.SelectedIndex = -1;
        }

        private void user_save_button_Click(object sender, EventArgs e)
        {
            if (name_input.Text == "" || surname_input.Text == "" || code_input.Text == "" || user_group_combo.SelectedIndex == -1)
            {
                NotifyUser("You must fill in all the fields before saving.");
                return;
            }

            // check if the user exists
            userController.AssignFromName(name_input.Text, surname_input.Text);

            if (userController.userObj == null)
            {
                // user with such credentials does not exist -> create new user
                // lookup the group for this user
                
                groupController.AssignFromName(user_group_combo.SelectedItem.ToString());

                if (groupController.groupObj == null)
                {
                    NotifyUser("Invalid user group.");
                    return;
                }

                // assign the user controller with the new user data
                userController.userObj = new UserModel
                {
                    name = name_input.Text,
                    surname = surname_input.Text,
                    code = code_input.Text,
                    groupID = groupController.groupObj.ID
                };

                userController.CreateUser();

                NotifyUser("User successfully created.");
                
                ResetGlobalObjects();
                ResetData();
            }

            // user exists -> model has been assigned -> update the user
            try
            {
                groupController.AssignFromName(user_group_combo.SelectedItem.ToString());
            }
            catch
            {

            }

            if (groupController.groupObj == null)
            {
                NotifyUser("Invalid user group.");
                return;
            }

            userController.UpdateUser(code_input.Text, groupController.groupObj.ID);

            NotifyUser("User data successfull updated.");

            ResetGlobalObjects();
            ResetData();
        }

        private void user_delete_button_Click(object sender, EventArgs e)
        {
            if (name_input.Text == "" || surname_input.Text == "" || code_input.Text == "" || user_group_combo.SelectedIndex == -1)
            {
                NotifyUser("You must fill in all the fields before saving.");
                return;
            }

            // check if the user exists
            userController.AssignFromName(name_input.Text, surname_input.Text);

            if (userController.userObj == null)
            {
                NotifyUser("User not found.");
                return;
            }

            groupController.AssignFromName(user_group_combo.SelectedItem.ToString());
            if (groupController.groupObj == null)
            {
                NotifyUser("Invalid user group.");
                return;
            }

            userController.DeleteUser();
            
            ResetGlobalObjects();
            ResetData();

            NotifyUser("User deleted successfully.");

        }

        private void group_group_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(group_group_combo.SelectedIndex == -1)
            {
                // return on no selection -> user will provide input
                return;
            }
            // reset data
            groupController.permissions.Clear();
            currentPermissions.Clear();
            groupController.groupObj = null;

            // on each select get permissions for that group
            PopulatePermissionsList();

            DeselectPermissionItems();

            // get the group of the selected index
            groupController.AssignFromName(group_group_combo.SelectedItem.ToString());

            if (groupController.groupObj == null)
            {
                NotifyUser("Invalid user group.");
                return;
            }

            // highlight the permissions for the selected group
            groupController.GetAllPermissions();
            
            foreach(PermissionController permission in groupController.permissions)
            {
                // check the listbox for match in the string content and highlight the matches
                for(int i = 0; i < group_permissions.Items.Count; i++)
                {
                    if (group_permissions.Items[i].ToString() == permission.GetString())
                    {
                        group_permissions.SetSelected(i, true);
                    }
                }
            }
        }

        private void DeselectPermissionItems()
        {
            // method for deselecting all items in the listbox
            for(int i = 0; i < group_permissions.Items.Count; i++)
            {
                group_permissions.SetSelected(i, false);
            }
        }

        private void group_save_button_Click(object sender, EventArgs e)
        {
            if (group_group_combo.SelectedIndex == -1)
            {
                if(group_group_combo.Text == "")
                {
                    NotifyUser("You must select a group or enter a new group name before saving.");
                    return;
                }
                // create new group object and save it with the currently selected permissions
                groupController.groupObj = new GroupModel
                {
                    name = group_group_combo.Text
                };
                
                foreach(var item in group_permissions.SelectedItems)
                {
                    // get the selected permissions and assign them to the group
                    foreach(PermissionController permission in currentPermissions)
                    {
                        if(permission.GetString() == item.ToString())
                        {
                            groupController.permissions.Add(permission);
                        }
                    }
                }

                int status = groupController.CreateNewGroup();

                if(status == 0)
                {
                    PopulateGroups(user_group_combo);
                    PopulateGroups(group_group_combo);
                    NotifyUser("Group successfully created.");
                }
                else
                {
                    NotifyUser("Error while creating group.");
                }

                return;
            }

            // user selected an index -> just update the new permissions
            Console.Error.WriteLine("Selected group is being updated ...");

            // get the group contreoller from selected
            groupController.AssignFromName(group_group_combo.SelectedItem.ToString());

            if(groupController.groupObj == null)
            {
                NotifyUser("Invalid user group.");
                return;
            }

            // clear the permissions
            groupController.permissions.Clear();

            // set new permissions
            foreach (var item in group_permissions.SelectedItems)
            {
                // get the selected permissions and assign them to the group
                foreach (PermissionController permission in currentPermissions)
                {
                    if (permission.GetString() == item.ToString())
                    {
                        groupController.permissions.Add(permission);
                    }
                }
            }

            // update the group
            groupController.UpdateGroup();
            ResetGroupData();
            NotifyUser("Group succesfully updated.");
        }

        private void ResetGroupData()
        {
            group_group_combo.SelectedIndex = -1;
            group_permissions.ClearSelected();

            groupController.permissions.Clear();
            groupController.groupObj = null;
        }

        private void group_delete_button_Click(object sender, EventArgs e)
        {
            // method for deleting a specified group
            if (group_group_combo.SelectedIndex == -1)
            {
                NotifyUser("You must select a group before deleting.");
                return;
            }

            // get the group contreoller from selected
            groupController.AssignFromName(group_group_combo.SelectedItem.ToString());

            if(groupController.groupObj == null)
            {
                NotifyUser("Invalid user group.");
                return;
            }

            groupController.DeleteGroup();
            ResetGroupData();
            PopulateGroups(user_group_combo);
            PopulateGroups(group_group_combo);
            NotifyUser("Group succesfully deleted.");
        }

        private void group_permissions_MouseDown(object sender, MouseEventArgs e)
        {
            // Check if the right mouse button is clicked
            if (e.Button == MouseButtons.Right)
            {
                // Get the index of the item at the mouse pointer's location
                int index = group_permissions.IndexFromPoint(e.Location);

                // If an item is found at the mouse pointer's location
                if (index != ListBox.NoMatches)
                {
                    // Unselect the item
                    group_permissions.SetSelected(index, false);
                }
            }
        }

        private void permission_combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // method for updating the groupbox input data to match permission data
            if(permission_combo.SelectedIndex == -1)
            {
                return;
            }

            // get the selected permission
            PermissionController permission = permissionController.GetPermissionByString(permission_combo.SelectedItem.ToString());
            if (permission == null)
            {
                NotifyUser("Invalid permission");
                return;
            }

            // populate the input fields
            start_day_numeric.Value = permission.permissionObj.startDay;
            end_day_numeric.Value = permission.permissionObj.endDay;
            start_time_input.Text = permission.permissionObj.startTime;
            end_time_input.Text = permission.permissionObj.endTime;
           
            // get the if of a reder from the combobobox
            string reader_name =  permission.GetReaderName();

            // get the item index to select from the combobox
            int selectedIndex = reader_combo.FindString(reader_name);

            // set the selected index
            reader_combo.SelectedIndex = selectedIndex;
        }

        private void permissions_save_button_Click(object sender, EventArgs e)
        {
            // check the permission combobox for index -> if -1 -> adding new permission, otherwise updating

            if (start_day_numeric.Value > 6 || end_day_numeric.Value > 6 || start_time_input.Text == "" || end_time_input.Text == "" || reader_combo.SelectedIndex == -1)
            {
                NotifyUser("You must fill in all the fields before saving.");
                return;
            }

            if (!DateTime.TryParse(start_time_input.Text, out DateTime start) || !DateTime.TryParse(end_time_input.Text, out DateTime end))
            {
                NotifyUser("Invalid time format.");
                return;
            }

            if (DateTime.Parse(start_time_input.Text) > DateTime.Parse(end_time_input.Text))
            {
                NotifyUser("Start time must be smaller than end time.");
                return;
            }

            if (start_day_numeric.Value > end_day_numeric.Value)
            {
                NotifyUser("Start day must be smaller than end day.");
                return;
            }

            if (permission_combo.SelectedIndex == -1)
            {
                // create new permission

                // get the reader id from the combobox
                readerController.AssignFromName(reader_combo.SelectedItem.ToString());

                if(readerController.readerObj == null)
                {
                    NotifyUser("Invalid reader.");
                    return;
                }

                // create new permission
                permissionController.CreateNewPermission((int)start_day_numeric.Value, (int)end_day_numeric.Value, start_time_input.Text, end_time_input.Text, readerController.readerObj.ID);
                
                ResetPermissionData();
                
                NotifyUser("Permission successfully created.");
                PopulatePermissionsList();
                PopulatePeermissionsCombo();
                return;
            }

            // update the permission
            PermissionController permission = permissionController.GetPermissionByString(permission_combo.SelectedItem.ToString());
            if (permission.permissionObj == null)
            {
                NotifyUser("Invalid permission");
                return;
            }

            // get the reader id from the combobox
            readerController.AssignFromName(reader_combo.SelectedItem.ToString());

            if (readerController.readerObj == null)
            {
                NotifyUser("Invalid reader.");
                return;
            }

            permissionController.UpdatePermission(permission.permissionObj.ID, (int)start_day_numeric.Value, (int)end_day_numeric.Value, start_time_input.Text, end_time_input.Text, readerController.readerObj.ID);
            ResetPermissionData();
            PopulatePermissionsList();
            PopulatePeermissionsCombo();
            NotifyUser("Permission successfully updated.");
        }

        public void ResetPermissionData()
        {
            permission_combo.Text = "";
            permission_combo.SelectedIndex = -1;
            start_day_numeric.Value = 0;
            end_day_numeric.Value = 0;
            start_time_input.Text = "";
            end_time_input.Text = "";
            reader_combo.SelectedIndex = -1;

            readerController.readerObj = null;
        }

        private void permissions_delete_button_Click(object sender, EventArgs e)
        {
            // method for deleting permissions from the databse
            if (permission_combo.SelectedIndex == -1)
            {
                NotifyUser("You must select a permission before deleting.");
                return;
            }

            // get the selected permission
            PermissionController permission = permissionController.GetPermissionByString(permission_combo.SelectedItem.ToString());
            if (permission.permissionObj == null)
            {
                NotifyUser("Invalid permission");
                return;
            }
            permissionController.DeletePermission(permission.permissionObj.ID);
            ResetPermissionData();
            PopulatePermissionsList();
            PopulatePeermissionsCombo();
            NotifyUser("Permission successfully deleted.");
        }
    }
}
