using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeScanner.Model
{
    public class TicketModel
    {
        public int ticketID { get; set; }
        public int eventID { get; set; }
        public string ticketCode { get; set; }
        public bool used { get; set; }
        public int saleID { get; set; }
    }
}
