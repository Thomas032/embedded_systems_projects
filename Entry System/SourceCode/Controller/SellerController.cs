using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;

namespace CodeScanner.Controller
{
    internal class SellerController
    {
        public SellerModel sellerObj { get; set; }

        public void AssignModel(SQLiteDataReader reader)
        {
            // function that accepts an SQL reader object as an argument and maps its data to internal seller object
            try
            { 
                sellerObj = new SellerModel
                {
                    sellerID = Convert.ToInt32(reader["sellerID"]),
                    sellerName = Convert.ToString(reader["sellerName"]),
                    sellerSurname = Convert.ToString(reader["sellerSurname"]),
                    sellerCode = Convert.ToString(reader["sellerCode"]),
                };
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Chyba při zpracovávání dat prodejce: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
