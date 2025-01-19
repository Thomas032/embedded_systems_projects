using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entra.Model
{
    public class PermissionModel
    {
        public int ID { get; set; }
        public int startDay { get; set; }
        public int endDay { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public int readerID { get; set; }
    }
}
