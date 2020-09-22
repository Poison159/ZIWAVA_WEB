using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class Artist
    {
        public int id { get; set; }
        public string name { get; set; }
        public string instaHandle { get; set; }
        [Required]
        public string imgPath { get; set; }
        [NotMapped]
        public HttpPostedFileBase imageUpload { get; set; }
    }
}