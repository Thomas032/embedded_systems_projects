using Entra.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Entra.Model
{
    public class ReaderModel
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string uuid { get; set; } // ideally uuid representing the decoding schema for regex
    }
}
