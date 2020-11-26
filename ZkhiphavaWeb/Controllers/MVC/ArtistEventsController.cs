using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZkhiphavaWeb.Models;

namespace ZkhiphavaWeb.Controllers.MVC
{
    [HandleError]
    [RequireHttps]
    public class ArtistEventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ArtistEvents
        public ActionResult Index()
        {
            ViewBag.eventNames = Helper.getEventNames(db.Events.ToList());
            ViewBag.artistNames = Helper.getArtistNames(db.Artists.ToList()); 
            return View(db.ArtistEvents.ToList());
        }

        // GET: ArtistEvents/Details/5
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArtistEvent artistEvent = db.ArtistEvents.Find(id);
            if (artistEvent == null)
            {
                return HttpNotFound();
            }
            return View(artistEvent);
        }

        // GET: ArtistEvents/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.artistId = new SelectList(db.Artists, "id", "name");
            ViewBag.eventId = new SelectList(db.Events, "id", "title");
            return View();
        }

        // POST: ArtistEvents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "id,artistId,eventId")] ArtistEvent artistEvent)
        {
            ViewBag.artistId = new SelectList(db.Artists, "id", "name");
            ViewBag.eventId = new SelectList(db.Events, "id", "title");
            if (ModelState.IsValid)
            {
                db.ArtistEvents.Add(artistEvent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(artistEvent);
        }

        // GET: ArtistEvents/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            ViewBag.artistId = new SelectList(db.Artists, "id", "name");
            ViewBag.eventId = new SelectList(db.Events, "id", "title");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArtistEvent artistEvent = db.ArtistEvents.Find(id);
            if (artistEvent == null)
            {
                return HttpNotFound();
            }
            return View(artistEvent);
        }

        // POST: ArtistEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "id,artistId,eventId")] ArtistEvent artistEvent)
        {
            ViewBag.artistId = new SelectList(db.Artists, "id", "name");
            ViewBag.eventId = new SelectList(db.Events, "id", "title");
            if (ModelState.IsValid)
            {
                db.Entry(artistEvent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(artistEvent);
        }

        // GET: ArtistEvents/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArtistEvent artistEvent = db.ArtistEvents.Find(id);
            if (artistEvent == null)
            {
                return HttpNotFound();
            }
            return View(artistEvent);
        }

        // POST: ArtistEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            ArtistEvent artistEvent = db.ArtistEvents.Find(id);
            db.ArtistEvents.Remove(artistEvent);
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
