using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class Image
    {
        [Required]
        public int id { get; set; }
        [Required]
        public int indawoId { get; set; }
        public string eventName { get; set; }
        [Required]
        public string imgPath { get; set; }
        [NotMapped]
        public HttpPostedFileBase imageUpload { get; set; }
    }
}