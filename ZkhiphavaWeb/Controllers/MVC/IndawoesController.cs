using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZkhiphavaWeb.Models;

namespace ZkhiphavaWeb.Controllers.mvc
{
    [HandleError]
    [RequireHttps]
    public class IndawoesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        public List<string> vibes       = new List<string>() { "Chilled", "Club", "Outdoor", "Pub/Bar" };
        public List<string> cities      = new List<string> { "Gauteng", "Western Cape", "KwaZulu-Natal"};
        public List<string> daysOfweek  = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        // GET: Indawoes
        public ActionResult Index(int? page, string name,string type, string province)
        {
            var vibesList   = new List<string>();
            var dbIndawoes  = from cr in db.Indawoes.Where(x => x.id != 9).ToList() select cr;
            var rnd         = new Random();
            var vibequery   = from gmq in db.Indawoes orderby gmq.type select gmq.type;
            var indawoes    = Helper.checkParams(name,type,province, dbIndawoes);
            
                
            vibesList.AddRange(vibequery.Distinct());
            //Helper.getAllImages(db.Images.ToList().Where(x => x.indawoId == 9).ToList(), db);
            ViewBag.selectedType = type;
            ViewBag.selectedProvince = province;
            ViewBag.type = new SelectList(vibesList);
            ViewBag.province = new SelectList(cities);
            ViewBag.Stats = db.AppStats.ToList().Last();
            ViewBag.Stats.counter += 1;
            int pageSize = 9;
            int pageNumber = (page ?? 1);
            if (!string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(province) || page > 1)
                indawoes = indawoes.ToList();
            else
                indawoes = indawoes.ToList().OrderBy(x => rnd.Next()).ToList();
            foreach (var indawo in indawoes)
                Helper.prepareLocation(indawo, db);
            return View(indawoes.ToPagedList(pageNumber, pageSize));
        }

        // GET: Indawoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Indawo indawo = db.Indawoes.Find(id);
            Helper.prepareLocation(indawo, db);
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }
        [Authorize]
        public ActionResult CreateImg(int? indawoId)
        {
            Image img = new Image();
            img.indawoId = (int)indawoId;
            return View(img);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImg([Bind(Include = "id,indawoId,imgPath,eventName")] Image image, int? indawoId)
        {
            
                image.indawoId = (int)indawoId;
                ViewBag.indawoId = new SelectList(db.Indawoes, "id", "name", image.indawoId);
                if (ModelState.IsValid)
                {
                    var randString = Helper.RandomString(10);
                    string targetPath = Server.MapPath("~");
                    var path = targetPath + @"Content\imgs\" + randString + ".png";
                    try
                    {
                        Helper.downloadImage(path, image.imgPath);
                        image.imgPath = randString + ".png";
                        db.Images.Add(image);
                        db.SaveChanges();
                        RedirectToAction("Details", "Indawoes", new { id = image.indawoId });
                    }
                    catch (Exception)
                    {
                        return View(image);
                    }
                }
            
            return View();
        }
        public ActionResult EditImg(int? id,int? indawoId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            image.indawoId = (int)indawoId;
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditImg([Bind(Include = "id,indawoId,imgPath,eventName")] Image image,int? indawoId)
        {
            image.indawoId = (int)indawoId;
            if (ModelState.IsValid)
            {
                var randString = Helper.RandomString(10);
                string targetPath = Server.MapPath("~");
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, image.imgPath);
                    image.imgPath = randString + ".png";
                    db.Entry(image).State = EntityState.Modified;
                    Helper.deleteOldImage(new List<Image> { image });
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return View(image);
                }
                return RedirectToAction("Details", "Indawoes", new { id = image.indawoId });
            }
            return View(image);
        }

        [Authorize]
        public ActionResult CreateOp(int? indawoId)
        {
            OperatingHours op = new OperatingHours();
            op.indawoId = (int)indawoId;
            ViewBag.day = new SelectList(daysOfweek);
            return View(op);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOp([Bind(Include = "id,indawoId,day,occation,openingHour,closingHour")] OperatingHours op, int? indawoId)
        {
            ViewBag.day = new SelectList(daysOfweek, op.day);
            if (ModelState.IsValid)
            {
                op.indawoId = (int)indawoId;
                db.OperatingHours.Add(op);
                db.SaveChanges();
                return RedirectToAction("Details", "Indawoes", new { id = op.indawoId });
            }

            return View();
        }

        [Authorize]
        public ActionResult EditOp(int id,int? indawoId)
        {
            var op = db.OperatingHours.ToList().First(x => x.id == id);
            op.indawoId = (int)indawoId;
            ViewBag.day = new SelectList(daysOfweek);
            return View(op);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditOp([Bind(Include = "id,indawoId,occation,day,openingHour,closingHour")] OperatingHours operatingHours, int? indawoId) {
            operatingHours.indawoId = (int)indawoId;
            ViewBag.indawoId = new SelectList(db.Indawoes, "id", "name", operatingHours.indawoId);
            ViewBag.day = new SelectList(daysOfweek, operatingHours.day);
            if (ModelState.IsValid)
            {
                if (operatingHours.closingHour.TimeOfDay.ToString().First() == '0')
                {
                    operatingHours.closingHour = operatingHours.closingHour.AddDays(1);
                }
                db.Entry(operatingHours).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(operatingHours);
        }

        [Authorize]
        public ActionResult CreateSp(int? indawoId)
        {
            SpecialInstruction sp = new SpecialInstruction();
            sp.indawoId = (int)indawoId;
            return View(sp);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSp([Bind(Include = "id,indawoId,instruction")] SpecialInstruction sp, int? indawoId)
        {
            if (ModelState.IsValid)
            {
                sp.indawoId = (int)indawoId;
                db.SpecialInstructions.Add(sp);
                db.SaveChanges();
                return RedirectToAction("Details", "Indawoes", new { id = sp.indawoId });
            }

            return View();
        }


        [Authorize]
        public ActionResult CreateEvent(int? indawoId)
        {
            Event @event = new Event();
            @event.indawoId = (int)indawoId;
            return View(@event);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent([Bind(Include = "id,indawoId,lat,lot,title,description,address,price,date,stratTime,endTime,imgPath")] Event @event, int? indawoId)
        {
            if (ModelState.IsValid)
            {
                @event.indawoId = (int)indawoId;
                db.Events.Add(@event);
                db.SaveChanges();
                return RedirectToAction("Index", "Indawoes", new { area = "" });
            }

            return View();
        }

        [Authorize]
        // GET: Indawoes/Create
        public ActionResult Create()
        {
            Indawo indawo = new Indawo();
            ViewBag.type = new SelectList(vibes);
            ViewBag.city = new SelectList(cities);
            return View(indawo);
        }

        // POST: Indawoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,type,rating,entranceFee,name,lat,lon,address,city,imgPath,instaHandle")] Indawo indawo)
        {
            ViewBag.type = new SelectList(vibes);
            ViewBag.city = new SelectList(cities);
            if (ModelState.IsValid)
            {
                if (db.Indawoes.ToList().Where(x => x.name == indawo.name && x.city == indawo.city).Count() == 0) {
                    var path = "default.png";

                    for (int i = 0; i <= 2; i++)
                        indawo.images.Add(new Image() { indawoId = indawo.id, imgPath = path });

                    db.Indawoes.Add(indawo);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Indawoes", new { id = indawo.id });
                } 
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public HttpStatusCode AddImage([Bind(Include = "indawoId,imageUpload")]IndawoImage indawoImage)
        {
            if (indawoImage.imageUpload != null)
            {
                try
                {
                    var indawo = db.Indawoes.Find(indawoImage.indawoId);
                    var indawoImages = db.Images.Where(x => x.indawoId == indawoImage.indawoId);
                    string fileName = Path.GetFileNameWithoutExtension(indawoImage.imageUpload.FileName);
                    string extention = Path.GetExtension(indawoImage.imageUpload.FileName);
                    fileName = indawo.name + (indawoImages.Count() + 1).ToString() + extention;
                    var imgPath = "~/Content/imgs/" + fileName;
                    if (indawoImages.Count() < 3) {
                        db.Images.Add(new Image { indawoId = indawoImage.indawoId, imgPath = imgPath});
                        db.SaveChanges();
                    }
                    indawo.imageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/imgs/"), fileName));
                    return HttpStatusCode.OK;
                }
                catch (Exception)
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            else {
                return HttpStatusCode.BadRequest;
            }
        }

        // GET: Indawoes/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            ViewBag.type = new SelectList(vibes);
            ViewBag.city = new SelectList(cities);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }

        // POST: Indawoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,type,rating,entranceFee,name,lat,lon,address,city,imgPath,instaHandle")] Indawo indawo)
        {
            ViewBag.type = new SelectList(vibes);
            ViewBag.city = new SelectList(cities);
            if (ModelState.IsValid)
            {
                db.Entry(indawo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(indawo);
        }

        // GET: Indawoes/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return HttpNotFound();
            }
            return View(indawo);
        }

        // POST: Indawoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Indawo indawo = db.Indawoes.Find(id);
            db.Indawoes.Remove(indawo);
            foreach (var item in db.Events.Where(x => x.indawoId == indawo.id)) { db.Events.Remove(item); }
            foreach (var item in db.Images.Where(x => x.indawoId == indawo.id)) { db.Images.Remove(item); }
            foreach (var item in db.OperatingHours.Where(x => x.indawoId == indawo.id)) { db.OperatingHours.Remove(item); }
            foreach (var item in db.SpecialInstructions.Where(x => x.indawoId == indawo.id)) { db.SpecialInstructions.Remove(item); }
            Helper.deleteOldImage(db.Images.Where(x => x.indawoId == indawo.id).ToList());
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
