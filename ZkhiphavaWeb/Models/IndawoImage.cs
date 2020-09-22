using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZkhiphavaWeb.Models
{
    public class IndawoImage
    {
        public HttpPostedFileBase imageUpload { get; set; }
        public int indawoId { get; set; }
    }
}