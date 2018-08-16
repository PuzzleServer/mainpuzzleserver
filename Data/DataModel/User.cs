using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
        public string EmployeeAlias { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string TShirtSize { get; set; }
        public bool VisibleToOthers { get; set; }
    }
}
