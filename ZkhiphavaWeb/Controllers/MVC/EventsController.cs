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
    
    public class EventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Events
        public ActionResult Index()
        {
            return View(db.Events.ToList());
        }

        // GET: Events/Details/5
        public ActionResult Details(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            @event.images = db.Images.Where(x => x.eventName == @event.title).ToList();
            var artistEvents = db.ArtistEvents.Where(x => x.eventId == @event.id);
            List<int> artisIds = new List<int>();
            var artists = new List<Artist>();
            foreach (var artEv in artistEvents){
                artisIds.Add(artEv.artistId);
            }
            foreach (var item in artisIds){
                var artist = db.Artists.First(x => x.id == item);
                if (artist != null)
                    @event.artists.Add(artist);
            }
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: Events/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "id,indawoId,title,description,date,stratTime,endTime,price,address,lat,lon,url,imgPath")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Events.Add(@event);
                db.SaveChanges();
                return RedirectToAction("Create","ArtistEvents");
            }

            return View(@event);
        }

        [Authorize]
        public ActionResult CreateImg(int? eventId)
        {
            Image img = new Image();
            img.indawoId = (int)eventId;
            var @event = db.Events.ToList().First(x => x.id == eventId);
            img.eventName = @event.title;
            return View(img);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImg([Bind(Include = "id,indawoId,imgPath,eventName")] Image img, int? eventId)
        {
            if (ModelState.IsValid)
            {
                img.indawoId = (int)eventId;
                var @event = db.Events.ToList().First(x => x.id == eventId);
                img.eventName = @event.title;

                var randString = Helper.RandomString(10);
                string targetPath = Server.MapPath("~");
                var path = targetPath + @"Content\imgs\" + randString + ".png";
                try
                {
                    Helper.downloadImage(path, img.imgPath);
                    img.imgPath = "http://zkhiphava.co.za/Content/imgs/" + randString + ".png";
                    db.Images.Add(img);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return View(img);
                }
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            return View();
        }
        // GET: Events/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "id,indawoId,lat,lon,,title,imgPath,description,date,stratTime,endTime,address,price,url")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(@event);
        }

        // GET: Events/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            
            Event @event = db.Events.Find(id);
            var toRemove = db.ArtistEvents.Where(x => x.eventId == id);
            foreach (var item in toRemove){
                db.ArtistEvents.Remove(item);
            }
            db.Events.Remove(@event);
            foreach (var image in db.Images) { if (image.eventName == @event.title) { db.Images.Remove(image); } }
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
