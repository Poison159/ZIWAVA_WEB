using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class UserData
    {
        public UserData() {
            locations = new List<Indawo>();
            events = new List<Event>();
        }
        public int userId { get; set; }
        public List<Indawo> locations { get; set; }
        public List<Event> events { get; set; }
    }
}