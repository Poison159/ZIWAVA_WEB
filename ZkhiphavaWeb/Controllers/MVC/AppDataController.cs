using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZkhiphavaWeb.Models;

namespace ZkhiphavaWeb.Controllers.MVC
{
    [HandleError]
    [RequireHttps]
    public class AppDataController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: AppData
        public ActionResult Index(string email)
        {
            var webUser = db.Users.ToList().First(x => x.Email  == email);
            var appUser = db.AppUsers.ToList().First(x => x.email == webUser.Email);
            var userData = new UserData();
            string url = System.Configuration.ConfigurationManager.AppSettings["prodUrl"];
            Indawo indawo = null;
            Event @event = null;
            var id = 0;
            if (!string.IsNullOrEmpty(appUser.LikesLocations)) {
                foreach (var indawoId in appUser.LikesLocations.Split(','))
                {
                    try
                    {
                        id = Convert.ToInt32(indawoId);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    indawo = db.Indawoes.First(x => x.id == id);
                     
                    userData.locations.Add(indawo);
                }
                foreach (var ndawo in userData.locations){
                    Helper.prepareLocation(ndawo, db);
                }
            }
            if (!string.IsNullOrEmpty(appUser.interestedEvents))
            {
                foreach (var eventId in appUser.interestedEvents.Split(','))
                {
                    try
                    {
                        id = Convert.ToInt32(eventId);
                        @event = db.Events.First(x => x.id == id);
                        var eventArtistIds = db.ArtistEvents.ToList().Where(x => x.eventId == @event.id);
                        @event.artists = Helper.getArtists(eventArtistIds, db);
                        @event.images = db.Images.Where(x => x.eventName.ToLower().Trim() == @event.title.ToLower().Trim()).ToList();
                        @event.date = Helper.treatDate(@event.date);
                        Helper.appendDomain(@event.images, url);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if(@event != null)
                        userData.events.Add(@event);
                }
            }
            ViewBag.email = email;
            return View(userData);
        }
        public ActionResult IndexTwo(string email)
        {
            var webUser = db.Users.ToList().First(x => x.Email == email);
            var appUser = db.AppUsers.ToList().First(x => x.email == webUser.Email);
            var userData = new UserData();
            Indawo indawo = null;
            Event @event = null;
            var id = 0;
            if (!string.IsNullOrEmpty(appUser.LikesLocations))
            {
                foreach (var indawoId in appUser.LikesLocations.Split(','))
                {
                    try
                    {
                        id = Convert.ToInt32(indawoId);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    indawo = db.Indawoes.First(x => x.id == id);

                    userData.locations.Add(indawo);
                }
                foreach (var ndawo in userData.locations){
                    Helper.prepareLocation(ndawo, db);
                }
            }
            if (!string.IsNullOrEmpty(appUser.interestedEvents))
            {
                foreach (var eventId in appUser.interestedEvents.Split(','))
                {
                    try
                    {
                        id = Convert.ToInt32(eventId);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    @event = db.Events.First(x => x.id == id);
                    var eventArtistIds = db.ArtistEvents.ToList().Where(x => x.eventId == @event.id);
                    @event.artists = Helper.getArtists(eventArtistIds, db);
                    @event.images = db.Images.Where(x => x.eventName.ToLower().Trim() == @event.title.ToLower().Trim()).ToList();
                    @event.date = Helper.treatDate(@event.date);
                    userData.events.Add(@event);
                }
            }
            return View(userData);
        }
    }
}

    