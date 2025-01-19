using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Entra.Model
{
    public class UserModel
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string surname { get; set;}
        public string code { get; set; }
        public int groupID { get; set; }
    }
}
