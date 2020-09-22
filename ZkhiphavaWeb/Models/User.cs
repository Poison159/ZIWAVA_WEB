using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobileNumber { get; set; }
        public string password { get; set; }
        public string LikesLocations { get; set; }
        public string interestedEvents { get; set; }
    }
}