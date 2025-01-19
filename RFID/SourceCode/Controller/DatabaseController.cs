using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using System.IO;
using System.Data.Common;
using Entra.Model;

namespace Entra.Controller
{
    public class DatabaseController
    {
        public string path { get; set; }
        public bool fillData { get; set; }
        private SQLiteConnection connection;
        private SQLiteCommand command;

        public DatabaseController(string userPath = "entra.db")
        {
            fillData = false;
            path = userPath;
            InitDatabase();
            CreateTables();

            if (fillData)
            {
                // fill the database with some data
                return;
            }
        }

        private void InitDatabase()
        {
            if (!File.Exists(path))
            {
                try
                {
                    SQLiteConnection.CreateFile(path); // create a db file to connect to
                    fillData = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occured: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            try
            {
                connection = new SQLiteConnection($"Data Source={path};Version=3;");
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            command = new SQLiteCommand(connection);
        }

        private void CreateTable(string tableName, string tableDefinition)
        {
            try
            {
                command.CommandText = $@"
                CREATE TABLE IF NOT EXISTS `{tableName}` (
                    {tableDefinition}
                )";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error creating table {tableName}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateTables()
        {
            CreateTable("groupPermission", @"
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                groupID INTEGER NOT NULL,
                permissionID INTEGER NOT NULL,
                FOREIGN KEY(groupID) REFERENCES 'group'(id),
                FOREIGN KEY(permissionID) REFERENCES permission(id)
            ");

            CreateTable("log", @"
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                userID INTEGER NOT NULL,
                readerID INTEGER NOT NULL,
                accessTime TEXT NOT NULL,
                accessResult TEXT NOT NULL,
                FOREIGN KEY(userID) REFERENCES user(id),
                FOREIGN KEY(readerID) REFERENCES reader(id)
            ");

            CreateTable("group", @"
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL
            ");

            CreateTable("user", @"
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                surname TEXT,
                code TEXT NOT NULL UNIQUE,
                groupID INTEGER NOT NULL,
                FOREIGN KEY(groupID) REFERENCES 'group'(id)
            ");

            CreateTable("reader", @"
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                uuid INTEGER NOT NULL,
                FOREIGN KEY(uuid) REFERENCES permission(readerID)
            ");

            CreateTable("permission", @"
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                startDay INTEGER NOT NULL,
                endDay INTEGER NOT NULL,
                startTime TEXT NOT NULL,
                endTime TEXT NOT NULL,
                readerID INTEGER NOT NULL,
                FOREIGN KEY(readerID) REFERENCES reader(id)
            ");

            // Index creation query for the 'log' table
            try
            {
                command.CommandText = @"
                    CREATE INDEX IF NOT EXISTS log_accesstime_readerid_index 
                    ON log(accessTime, readerID)";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error creating index on 'log' table: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void InsertLog(int userID, int readerID, DateTime accessTime, bool accessResult)
        {
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"INSERT INTO 'log' (userID, readerID, accessTime, accessResult) VALUES ({userID}, {readerID}, '{accessTime.ToString("yyyy-MM-dd HH:mm:ss")}', '{(accessResult ? "granted" : "denied")}');";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error inserting log: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void InsertNewPermission(int startDay, int endDay, string startTime, string endTime, int readerID)
        {
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"INSERT INTO 'permission' (startDay, endDay, startTime, endTime, readerID) VALUES ({startDay}, {endDay}, '{startTime}', '{endTime}', {readerID});";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error inserting permission: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeletePermission(int id)
        {
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"DELETE FROM 'permission' WHERE id = {id};";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error deleting permission: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdatePermission(int id, int startDay, int endDay, string startTime, string endTime, int readerID)
        {
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"UPDATE 'permission' SET startDay = {startDay}, endDay = {endDay}, startTime = '{startTime}', endTime = '{endTime}', readerID = {readerID} WHERE id = {id};";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error updating permission: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public SQLiteDataReader GetGroup(int groupID)
        {
            Console.Error.WriteLine("Getting group ...");
            try
            {
                using (command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"SELECT * FROM [group] WHERE id = @id";
                    command.Parameters.AddWithValue("@id", groupID);

                    SQLiteDataReader result = command.ExecuteReader();
                    if (result != null)
                    {
                        if (result.Read())
                        {
                            Console.Error.WriteLine("DEBUGGED = " + Convert.ToString(result["name"]));
                            return result;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite 30 error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An 30 error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public List<int> GetGroupPermissionIDs(int groupID)
        {
            List<int> permissionIDs = new List<int>();

            try
            {
                using (command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"SELECT permissionID FROM groupPermission WHERE groupID = {groupID}";

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissionIDs.Add(Convert.ToInt32(reader["permissionID"]));
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return permissionIDs;
        }

        public List<string> GetReaderNames()
        {
            List<string> readerNames = new List<string>();

            try
            {
                using (command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT name FROM reader";

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            readerNames.Add(reader["name"].ToString());
                        }
                    }
                }

                return readerNames;
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return readerNames; // Return an empty list in case of error
        }


        public int CreateNewGroup(string name, List<PermissionController> permissions)
        {
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"INSERT INTO [group] (name) VALUES (@name);";
                    command.Parameters.AddWithValue("@name", name);
                    command.ExecuteNonQuery();

                    // Get the latest group ID from the group table
                    int groupID = GetGroupId(name);

                    if (groupID == -1)
                    {
                        // Return error flag
                        return -1;
                    }

                    // Insert permissions for the group
                    foreach (PermissionController permission in permissions)
                    {
                        using (SQLiteCommand permissionCommand = new SQLiteCommand(connection))
                        {
                            permissionCommand.CommandText = $"INSERT INTO groupPermission (groupID, permissionID) VALUES (@groupID, @permissionID);";
                            permissionCommand.Parameters.AddWithValue("@groupID", groupID);
                            permissionCommand.Parameters.AddWithValue("@permissionID", permission.permissionObj.ID);
                            permissionCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1; // Return error flag
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1; // Return error flag
            }
            return 0;
        }


        public void UpdateGroup(int id, string name, List<PermissionController> permissions)
        {
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"UPDATE [group] SET name = @name WHERE id = @id";
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();

                    // Delete all permissions for the group
                    using (SQLiteCommand deleteCommand = new SQLiteCommand(connection))
                    {
                        deleteCommand.CommandText = $"DELETE FROM groupPermission WHERE groupID = @groupID";
                        deleteCommand.Parameters.AddWithValue("@groupID", id);
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Insert permissions for the group
                    foreach (PermissionController permission in permissions)
                    {
                        using (SQLiteCommand permissionCommand = new SQLiteCommand(connection))
                        {
                            permissionCommand.CommandText = $"INSERT INTO groupPermission (groupID, permissionID) VALUES (@groupID, @permissionID);";
                            permissionCommand.Parameters.AddWithValue("@groupID", id);
                            permissionCommand.Parameters.AddWithValue("@permissionID", permission.permissionObj.ID);
                            permissionCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteGroup(int id)
        {
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"DELETE FROM [group] WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();

                    // Delete all permissions for the group
                    using (SQLiteCommand deleteCommand = new SQLiteCommand(connection))
                    {
                        deleteCommand.CommandText = $"DELETE FROM groupPermission WHERE groupID = @groupID";
                        deleteCommand.Parameters.AddWithValue("@groupID", id);
                        deleteCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int GetGroupId(string name)
        {
            try
            {
                using (command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"SELECT id FROM [group] WHERE name = @name";
                    command.Parameters.AddWithValue("@name", name);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return -1;
        }


        public List<PermissionModel> GetAllPermissions()
        {
            List<PermissionModel> permissions = new List<PermissionModel>();

            try
            {
                using (command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT * FROM permission";
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(new PermissionModel
                            {
                                ID = Convert.ToInt32(reader["id"]),
                                startDay = Convert.ToInt32(reader["startDay"]),
                                endDay = Convert.ToInt32(reader["endDay"]),
                                startTime = Convert.ToString(reader["startTime"]),
                                endTime = Convert.ToString(reader["endTime"]),
                                readerID = Convert.ToInt32(reader["readerID"])
                            });
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return permissions;
        }
        public string GetReaderName(int readerID)
        {
            // method for getting the name of the reader from the database
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT name FROM reader WHERE id = {readerID};";

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // if has data -> return the reader
                    return reader["name"].ToString();
                }
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return "";
        }

        public SQLiteDataReader GetModelReader(string table, string column, string code)
        {
            SQLiteDataReader reader = null;

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"SELECT * FROM [{table}] WHERE {column} = @code";
                    command.Parameters.AddWithValue("@code", code);

                    reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return reader;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public SQLiteDataReader GetReaderWithName(string name)
        {
            // get reader object from db with just the reader name

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM reader WHERE name = '{name}';";

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // if has data -> return the reader
                    // get reader name and print it to console
                    Console.WriteLine(reader["name"]);
                    return reader;
                }
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public SQLiteDataReader GetReaderWithAddress(int address)
        {
            // get reader object from db with just the reader name

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM reader WHERE uuid = '{address}';";

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // if has data -> return the reader
                    // get reader name and print it to console
                    Console.WriteLine(reader["uuid"]);
                    return reader;
                }
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }


        public SQLiteDataReader GetUserByName(string name, string surname)
        {
            // function that select the user with name and surname form database and returns its reader
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM 'user' WHERE name = '{name}' AND surname = '{surname}';";

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // if has data -> return the reader
                    return reader;
                }
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite errpr: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public SQLiteDataReader GetUserByCode(string code)
        {
            // function that returns the reader of the user with the specific code
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM 'user' WHERE code = '{code}';";

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // if has data -> return the reader
                    return reader;
                }
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite errpr: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }
        public List<string> GetGroupNames()
        {
            List<string> groupNames = new List<string>();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT name FROM [group]";

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string groupName = reader["name"].ToString();
                            groupNames.Add(groupName);
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return groupNames;
        }



        public int Entries(string table)
        {
            int index = 0;
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM '{table}';";

                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // if has data -> return the reader
                    index++;
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba při čtení kódu prodejce: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return index; // no data  = null
        }

        public void InsertUser(string name, string surname, string code, int groupID)
        {
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"INSERT INTO 'user' (name, surname, code, groupID) VALUES ('{name}', '{surname}', '{code}', {groupID});";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateUser(int id, string name, string surname, string code, int groupID)
        {
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"UPDATE 'user' SET name = '{name}', surname = '{surname}', code = '{code}', groupID = {groupID} WHERE id = {id};";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteUser(int id)
        {
            // method that deletes the user with the specific id
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"DELETE FROM 'user' WHERE id = {id};";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"SQLite error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"General error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
