using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class Request
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string description { get; set; }
    }
}