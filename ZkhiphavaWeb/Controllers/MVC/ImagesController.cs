using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZkhiphavaWeb.Models;

namespace ZkhiphavaWeb.Controllers.mvc
{
    public class ImagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Images
        public ActionResult Index(int? page, string indawoId, string eventName)
        {
            var Images = db.Images.ToList();
            var indawoNames = Helper.getIndawoNames(db.Indawoes.ToList());
            var eventNames = Helper.getEventNames(db.Events.ToList());
            var namesList = new List<string>();
            var eventList = new List<string>();
            if (!string.IsNullOrEmpty(indawoId)) {
                var indawo = db.Indawoes.First(x => x.name == indawoId);
                Images = Images.Where(x => x.indawoId == indawo.id).ToList();
            }
            if (!string.IsNullOrEmpty(eventName)){
                Images = Images.Where(x => x.eventName != null && x.eventName.Equals(eventName)).ToList();
            }
            ViewBag.indawoNames = indawoNames;
            foreach (var keyPair in indawoNames){
                namesList.Add(keyPair.Value);
            }
            foreach (var keyPair in eventNames){
                eventList.Add(keyPair.Value);
            }
            ViewBag.eventName       = new SelectList(eventList);
            ViewBag.indawoId        = new SelectList(namesList);
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            return View(Images.ToPagedList(pageNumber, pageSize));
        }

        // GET: Images/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // GET: Images/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.indawoId = new SelectList(db.Indawoes, "id", "name");
            return View();
        }

        // POST: Images/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,indawoId,imgPath,eventName")] Image image)
        {
            var prodPath = "https://zkhiphava.co.za/Content/imgs/";
            var testPath = "https://zkhiphavaweb.conveyor.cloud/Content/imgs/";
            ViewBag.indawoId = new SelectList(db.Indawoes, "id", "name", image.indawoId);
            if (ModelState.IsValid)
            {
                var randString = Helper.RandomString(10);
                string targetPath = Server.MapPath("~");
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, image.imgPath);
                    image.imgPath = prodPath + randString + ".png";
                    db.Images.Add(image);
                    db.SaveChanges();
                    RedirectToAction("Details", "Indawoes", new { id = image.indawoId });
                }
                catch (Exception)
                {
                    return View(image);
                }
            }

            return View(image);
        }

        // GET: Images/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "id,indawoId,imgPath,eventName")] Image image)
        {
            if (ModelState.IsValid)
            {
                var prodPath = "https://zkhiphava.co.za/Content/imgs/";
                var testPath = "https://zkhiphavaweb.conveyor.cloud/Content/imgs/";
                var randString = Helper.RandomString(10);
                string targetPath = Server.MapPath("~");
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try{
                    Helper.downloadImage(path, image.imgPath);
                    image.imgPath = testPath + randString + ".png";
                    db.Entry(image).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", "Indawoes", new { id = image.indawoId });
                }
                catch (Exception){
                    return View(image);
                }
                
            }
            return View(image);
        }

        // GET: Images/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Image image = db.Images.Find(id);
            db.Images.Remove(image);
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
