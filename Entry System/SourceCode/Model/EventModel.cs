using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeScanner.Model
{
    public class EventModel
    {
        public int eventID { get; set; }
        public string eventName { get; set; }
        public DateTime eventDate { get; set; }
        public int eventCapacity { get; set; }
        public float eventTicketPrice { get; set; }
        public string eventCode { get; set; }
    }
}
