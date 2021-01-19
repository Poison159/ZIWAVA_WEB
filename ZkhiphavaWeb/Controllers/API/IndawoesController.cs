using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using ZkhiphavaWeb;
using ZkhiphavaWeb.Models;
using Microsoft.Owin.Host.SystemWeb;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ZkhiphavaWeb.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    
    public class IndawoesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: api/Indawoes

        public List<Indawo> GetIndawoes(string userLocation, string distance, string vibe, string city)
        {
            var lon = userLocation.Split(',')[0];
            var lat = userLocation.Split(',')[1];
            var vibes = new List<string>() { "Chilled", "Club", "Outdoor", "Pub/Bar" };
            var filters = new List<string>() { "distance", "rating", "damage" };
            var rnd = new Random();
            List<Indawo> locations = null;

            Helper.IncrementAppStats(db,vibe.ToLower().Trim());
            if (userLocation.Split(',')[0] == "undefined") {
                return null;
            }
            if (string.IsNullOrEmpty(vibe)){
                return null;
            }
            locations = db.Indawoes.ToList().Where(x => x.city.Trim().ToLower() == city.Trim().ToLower() 
            && x.type.Trim().ToLower() == vibe.Trim().ToLower()).OrderBy(x => rnd.Next()).ToList();

            var listOfIndawoes = Helper.GetNearByLocations(lat, lon, Convert.ToInt32(distance), locations); 
            foreach (var ndawo in listOfIndawoes){
                Helper.prepareLocation(ndawo, db);
            }
            return listOfIndawoes.Where(x => x.id != 9).ToList();
        }
            
        [Route("api/GetByName")]
        [HttpGet]
        public object getByName(string name,string lat, string lon) {
           var locations    = db.Indawoes.ToList().Where(x => x.name.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
            foreach (var loc in locations)
            {
                loc.distance = Math.Round(Helper.distanceToo(Convert.ToDouble(lat, CultureInfo.InvariantCulture),
                    Convert.ToDouble(lon, CultureInfo.InvariantCulture),
                    Convert.ToDouble(loc.lat, CultureInfo.InvariantCulture),
                    Convert.ToDouble(loc.lon, CultureInfo.InvariantCulture), 'K'));
                Helper.prepareLocation(loc, db);
            }
                
            var events   = db.Events.ToList().Where(x => x.title.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();
            foreach (var evnt in events){
                Helper.prepareEvent(lat, lon, evnt, db);
            }
            return new { liked = locations.OrderByDescending(x => x.distance), interested = events.OrderByDescending(x => x.distance) };
        }

        [Route("api/GetDistance")]
        [HttpGet]
        public double getDistance(double lat1, double lon1, int indawoId)
        {
           double ret = 0;
            try{
                var indawo = db.Indawoes.Find(indawoId);
                ret = Math.Round(Helper.distanceToo(lat1, lon1,
                    Convert.ToDouble(indawo.lat, CultureInfo.InvariantCulture),
                    Convert.ToDouble(indawo.lon, CultureInfo.InvariantCulture), 'K'));
            }
            catch (Exception){
            }
            return ret;
        }

        [Route("api/IncIndawoStats")]
        [HttpGet]
        public void IncDirStats(int indawoId, string plat)
        {
            var tempStat = new IndawoStat() { indawoId = indawoId };
            if (db.IndawoStats.Count() == 0) {
                
                if (plat == "maps")
                    tempStat.dirCounter = 1;
                if (plat == "insta")
                    tempStat.instaCounter = 1;
                db.IndawoStats.Add(tempStat);
            }
            else
            {
                var indawoStats = db.IndawoStats.Where(x => x.indawoId == indawoId).ToList();
                if (indawoStats.Count() != 0 && indawoStats.Last().dayOfWeek == DateTime.Now.DayOfWeek)// if it's same day the find the last one to be made & increament
                {
                    if (plat == "maps")
                        indawoStats.Last().dirCounter++;
                    if (plat == "insta")
                        indawoStats.Last().instaCounter++;
                }
                else { // else create new
                    
                    if (plat == "maps")
                        tempStat.dirCounter = 1;
                    if (plat == "insta")
                        tempStat.instaCounter = 1;
                    db.IndawoStats.Add(tempStat);
                }
            }
            db.SaveChanges();
        }

        [Route("api/Request")]
        [HttpPost]
        public string getRequest([FromBody]Request request)
        {
            try{
                db.Requests.Add(request);
                db.SaveChanges();
                return "Thanks we got your message. We will be in touch so watch your emails😉";
            }
            catch (Exception){
                return "Something went wrong😢";
            }
        }

        [Route("api/UploadUser")]
        [HttpPost]
        public string getRequest([FromBody] User request)
        {
            try
            {
                db.AppUsers.Add(request);
                db.SaveChanges();
                return "Thanks we got your message. We will be in touch so watch your emails😉";
            }
            catch (Exception)
            {
                return "Something went wrong😢";
            }
        }

        [Route("api/MidRequest")]
        [HttpPost]
        public string getMidRequest([FromBody] Midworld request)
        {
            try
            {
                db.MidRequests.Add(request);
                db.SaveChanges();
                return "Thanks we got your message. We will be in touch so watch your emails😉";
            }
            catch (Exception)
            {
                return "Something went wrong😢";
            }
        }

        [Route("api/CheckToken")]
        [HttpGet]
        public object getToken(string email, string password)
        {
            var userStore = new UserStore<IdentityUser>();
            var userManager = new UserManager<IdentityUser>(userStore);
            IdentityUser user = null;
            User appUser = null;
            try{
                user = db.Users.First(x => x.Email.Trim().ToLower() == email.Trim().ToLower());
                appUser = db.AppUsers.First(x => x.email.ToLower().Equals(email.ToLower().Trim()));
            }
            catch {
                return new { Errors = "User not Found" };
            }
            var isMatch = userManager.CheckPassword(user, password);
            if (isMatch)
            {
                var token = db.Tokens.ToList().First(x => x._userId == email);
                token._grantDate = DateTime.Now;
                token._expiryDate.AddDays(60);
                db.SaveChanges();
                if (token != null)
                {
                    return new {token = token, user = appUser };
                }
                else
                {
                    return new { Errors = "Error logging in user" };
                }
            }
            else {
                return new { Errors = "Password incorrect" };
            }
        }

        [Route("api/RegisterUser")]
        [HttpGet]
        public object Register(string email, string password) {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            var user = new ApplicationUser();
            var appUser = new User() { name = email, email = email, password = Helper.GetHashString(password) };
            user.Email = email;
            user.UserName = email;
            var result = UserManager.Create(user, password);
            if (result.Succeeded)
            {
                db.AppUsers.Add(appUser);
                db.SaveChanges();
                var token = Helper.saveAppUserAndToken(user,db);
                return  new {token = token, user =  appUser};
            }
            else {
                return  result;
            }
        }

        [Route("api/Favorites")]
        [HttpGet]
        
        public List<Indawo> getFavorites(string idString)
        {
            var fav = new List<Indawo>();
            foreach (var item in idString.Split(',').Where(x => x != ""))
            {
                fav.Add(db.Indawoes.Find(Convert.ToInt32(item)));
            }
            return fav;
        }

        [Route("api/addFavorite")]
        [HttpGet]
        public void addFavorite(string email, int indawoId) {
            
            var user = db.AppUsers.First(x => x.email == email);
            if (user.LikesLocations == null) {
                user.LikesLocations += indawoId.ToString() + ",";
            }
            if(!user.LikesLocations.Split(',').Contains(indawoId.ToString()))
                user.LikesLocations += indawoId.ToString() + ",";
            db.SaveChanges();
        }

        [Route("api/removeFavorite")]
        [HttpGet]
        public void removeFavorite(string email, int indawoId)
        {

            var user = db.AppUsers.First(x => x.email == email);
            if (user.LikesLocations.Split(',').Contains(indawoId.ToString()))
                user.LikesLocations = Helper.BindSplit(user.LikesLocations.Split(',').Where(x => x != indawoId.ToString()));
            db.SaveChanges();
        }

        [Route("api/addInterested")]
        [HttpGet]
        public void addInterested(string email, int eventId)
        {

            var user = db.AppUsers.First(x => x.email == email);
            if (user.interestedEvents == null)
            {
                user.interestedEvents += eventId.ToString() + ",";
            }
            if (!user.interestedEvents.Split(',').Contains(eventId.ToString()))
                user.interestedEvents += eventId.ToString() + ",";
            db.SaveChanges();
        }

        [Route("api/removeInterested")]
        [HttpGet]
        public void removeInterested(string email, int eventId)
        {

            var user = db.AppUsers.First(x => x.email == email);
            if (user.interestedEvents.Split(',').Contains(eventId.ToString()))
                user.interestedEvents = Helper.BindSplit(user.interestedEvents.Split(',').Where(x => x != eventId.ToString()));
            db.SaveChanges();
        }



        [Route("api/addInterested")]
        [HttpGet]
        public void addInterested(int userId, int eventId)
        {
            var user = db.AppUsers.Find(userId);
            user.interestedEvents += eventId.ToString() + ",";
            db.SaveChanges();
        }

        [Route("api/getUserData")]
        [HttpGet]
        public object getUserData(string email,double lat, double lon)
        {
            try
            {
                var user = db.AppUsers.First(x => x.email == email);
                var res = Helper.LiekdFromString(user.LikesLocations, user.interestedEvents, db.Indawoes.ToList(), db.Events.ToList(), db, lat, lon);
                return res;
            }
            catch (Exception)
            {
                return null;
            }
        }


        //[Route("api/Event")]
        //[HttpGet]
        
        //public Event Event(int id,string lat, string lon)
        //{
        //    var evnt = db.Events.Find(id);
        //    try
        //    {
        //        Helper.prepareEvent(lat, lon, evnt, db);
        //        return evnt;
        //    }
        //    catch {
        //        return null;
        //    }   
        //}

        //[Route("api/Events")]
        //[HttpGet]
        //public List<Event> Events(string lat, string lon)
        //{
        //    int outPut;
        //    var rnd = new Random();
        //    try { 
        //        var events = db.Events.Where(x =>x.indawoId == 9).ToList();
        //        foreach (var evnt in events)
        //        {
        //            if (int.TryParse(lat[1].ToString(), out outPut) && int.TryParse(lon[0].ToString(), out outPut))
        //            {
        //                Helper.prepareEvent(lat, lon, evnt, db);
        //            }
        //        }
        //        var randEvents = events.OrderBy(x => rnd.Next()).ToList();
        //        Helper.convertDates(randEvents);
        //        return randEvents;
        //    }
        //    catch {
        //        return null;
        //    }
        //}



        // GET: api/Indawoes/5
        [ResponseType(typeof(Indawo))]
        
        public IHttpActionResult GetIndawo(int id)
        {
            Indawo indawo = db.Indawoes.Find(id);
            Helper.prepareLocation(indawo,db);
            if (indawo == null)
            {
                return NotFound();
            }
            return Ok(indawo);
        }

        // PUT: api/Indawoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutIndawo(int id, Indawo indawo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != indawo.id)
            {
                return BadRequest();
            }

            db.Entry(indawo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IndawoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        private List<Indawo> getIndawoWithIn50k(string userLocation)
        {
            //Using only userLocation return a list of places with in 50K of location
            throw new NotImplementedException();
        }

        // DELETE: api/Indawoes/5
        [ResponseType(typeof(Indawo))]
        public IHttpActionResult DeleteIndawo(int id)
        {
            Indawo indawo = db.Indawoes.Find(id);
            if (indawo == null)
            {
                return NotFound();
            }

            db.Indawoes.Remove(indawo);
            db.SaveChanges();

            return Ok(indawo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IndawoExists(int id)
        {
            return db.Indawoes.Count(e => e.id == id) > 0;
        }
    }
}