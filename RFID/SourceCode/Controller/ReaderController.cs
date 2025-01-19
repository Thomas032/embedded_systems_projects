using Entra.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Entra.Controller
{
    public class ReaderController
    {
        public ReaderModel readerObj { get; set; }
        private DatabaseController dbController;

        public ReaderController(DatabaseController databaseController)
        {
            dbController = databaseController;
            readerObj = new ReaderModel();
        }

        public void AssignModel(SQLiteDataReader reader)
        {
            try
            {
                readerObj = new ReaderModel
                {
                    ID = Convert.ToInt32(reader["id"]),
                    name = Convert.ToString(reader["name"]),
                    uuid = Convert.ToString(reader["uuid"])
                };
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error occured: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AssignFromAddress(int address)
        {
            // compare the current reader address and assign the local reader model

            SQLiteDataReader rdr = dbController.GetReaderWithAddress(address);
            
            if(rdr == null)
            {
                readerObj = null;
                return;
            }

            // get the address(uuid) of the memory and convert it to integer
            int readerUUID = Convert.ToInt32(rdr["uuid"]);
            AssignModel(rdr);

        }

        public void AssignFromName(string name)
        {
            SQLiteDataReader rdr = dbController.GetReaderWithName(name);
            if(rdr == null)
            {
                MessageBox.Show($"No data in database -> unable to create reader model", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AssignModel(rdr);
        }

        public List<string> GetReaderNames()
        {
            // get all the reader names from the database
            List<string> readerNames = dbController.GetReaderNames();
            return readerNames;
        }
    }
}
