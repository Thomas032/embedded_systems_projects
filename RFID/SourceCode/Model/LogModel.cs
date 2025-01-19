using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entra.Model
{
    public class LogModel
    {
        public int ID { get; set; }
        public int userID { get; set; }
        public int readerID { get; set; }
        public DateTime accessTime { get; set; }
        public string accessResult { get; set; }
    }
}
