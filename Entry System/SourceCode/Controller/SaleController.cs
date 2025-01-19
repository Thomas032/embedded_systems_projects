using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace CodeScanner.Controller
{
    internal class SaleController
    {
        DatabaseController dbController { get; set; }
        public SaleModel saleObj { get; set; }

        public SaleController(DatabaseController databaseController)
        {
            dbController = databaseController;
        }

        public void CreateSale(SellerModel saleSeller)
        {
            // function for creating SaleModel object, assigning it some data and saving it to memory
            saleObj = new SaleModel();
            saleObj.sellerID = saleSeller.sellerID;
            saleObj.date = DateTime.Now;
            saleObj.saleID = dbController.Entries("Sale") + 1; // +1 due to indexing 
        }
    }
}
