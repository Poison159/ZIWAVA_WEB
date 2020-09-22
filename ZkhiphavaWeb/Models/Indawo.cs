using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class Indawo
    {
        public Indawo()
        {
            images = new List<Image>();
            operatingHoursStr = new List<string>();
            imgPath = "~/Content/user.png";
            open = true;
            closingSoon = false;
        }
        [Required]
        public int id { get; set; }
        [Required]
        public string type { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public double rating { get; set; }
        [Required]
        public double entranceFee { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string lat { get; set; }
        [Required]
        public string lon { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string imgPath { get; set; }
        [Required]
        public string instaHandle { get; set; }
        [MaxLength(7)]
        public OperatingHours[] oparatingHours { get; set; }
        public List<SpecialInstruction> specialInstructions { get; set; }
        
        public List<Image> images { get; set; }
        public List<Event> events { get; set; }
        [NotMapped]
        public HttpPostedFileBase imageUpload { get; set; }
        [NotMapped]
        public double distance { get; set; }
        public string info { get; set; }
        public string openOrClosedInfo { get; set; }
        public List<string> operatingHoursStr { get; set; }
        public bool open { get; set; }
        public bool closingSoon { get; set; }
        public bool openingSoon { get; set; }

    }
}