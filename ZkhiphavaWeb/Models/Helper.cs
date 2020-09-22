using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Device.Location;
using System.Net;

namespace ZkhiphavaWeb.Models
{
    public static class Helper
    {
        private static Random random = new Random();
        public static List<Indawo> GetNearByLocations(string Currentlat, string Currentlng, int distance, List<Indawo> indawo)
        {
            try
            {
                var userLocationLat = Convert.ToDouble(Currentlat, CultureInfo.InvariantCulture);
                var userLocationLong = Convert.ToDouble(Currentlng, CultureInfo.InvariantCulture);

                foreach (var item in indawo)
                {
                    var locationLat = Convert.ToDouble(item.lat, CultureInfo.InvariantCulture);
                    var locationLon = Convert.ToDouble(item.lon, CultureInfo.InvariantCulture);
                    var distanceToIndawo = distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K');
                    item.distance = Math.Round(distanceToIndawo);
                }
                List<Indawo> nearLocations = getPlacesWithIn(indawo, distance);
                return nearLocations;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        

        public static Coordinates getLatLon() {
            double lat = 0;
            double lon = 0;
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.Start(); //started watcher
            GeoCoordinate coord = watcher.Position.Location;
            if (!watcher.Position.Location.IsUnknown)
            {
                 lat = coord.Latitude; //latitude
                 lon = coord.Longitude;  //logitude
            }
            return new Coordinates(lat, lon);
        }

        public static Dictionary<string, List<OperatingHours>> getIndivisualOperationhours(List<OperatingHours> OpHours, List<Indawo> indawoes)
        {
            var izindawo = Helper.getIndawoNames(indawoes);
            var retHours = new Dictionary<string, List<OperatingHours>>();

            foreach (var indawo in izindawo)
            {
                retHours.Add(indawo.Value, OpHours.Where(x => x.indawoId == indawo.Key).ToList());
            }
            return retHours;
        }


        internal static List<OperatingHours> checkOPeratingHours(List<OperatingHours> opHours, List<Indawo> izindawo, ApplicationDbContext db)
        {
            var retOpHours = new List<OperatingHours>();
            var ids = getIndawoIds(izindawo);
            foreach (var opHour in opHours)
            {
                if (!ids.Contains(opHour.indawoId))
                {
                    db.OperatingHours.Remove(opHour);
                }
            }
            db.SaveChanges();
            return db.OperatingHours.ToList();
        }

        private static List<int> getIndawoIds(List<Indawo> izindawo)
        {
            var ret = new List<int>();
            foreach (var item in izindawo) { ret.Add(item.id); }
            return ret;
        }

        public static List<Image> LoadJson(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<Image> items = JsonConvert.DeserializeObject<List<Image>>(json);
                return items;
            }
        }
        public static string getLocationInfo(Indawo location)
        {

            if (location.distance > 1 && location.entranceFee > 0)
            {
                return location.distance + "KM | " + "R" + location.entranceFee;
            }
            else if (location.distance <= 1 && location.entranceFee == 0)
            {
                return "LESS THAN A KM AWAY | " + "FREE ";
            }
            else if (location.distance <= 1 && location.entranceFee > 0)
            {
                return "LESS THAN A KM AWAY | " + "R" + location.entranceFee;
            }
            else if (location.distance > 1 && location.entranceFee == 0)
            {
                return location.distance + "KM | " + "FREE";
            }
            else
            {
                return "";
            }
        }

        public static string getClosedStatus(Indawo location)
        {
            if (location.open == false && location.closingSoon == false && location.openingSoon == false)
            {
                return "CLOSED";
            }
            else if (location.open == true && location.closingSoon == true)
            {
                return "CLOSING SOON";
            }
            else if (location.open == false && location.openingSoon == true)
            {
                return "OPENING SOON";
            }
            else if (location.open == true && location.closingSoon == false)
            {
                return "OPEN";
            }
            return "";
        }

        internal static void getOpratingHoursStr(Indawo item)
        {
            var str = "";
            List<string> retStr = new List<string>();
            int i = 0;
            var operatingHours = item.oparatingHours;
            foreach (var opHour in operatingHours)
            {
                i++;
                str += opHour.day + " | " + opHour.openingHour.TimeOfDay.ToString().Split(':').Take(2).First() + ":" + opHour.openingHour.TimeOfDay.ToString().Split(':').Take(2).ElementAt(1) + " to "
                    + opHour.closingHour.TimeOfDay.ToString().Split(':').Take(2).First() + ":" + opHour.openingHour.TimeOfDay.ToString().Split(':').Take(2).ElementAt(1) + " " + opHour.occation;
                item.operatingHoursStr.Add(str);
                str = "";
            }
        }

        internal static void IncrementAppStats(ApplicationDbContext db, string vibe)
        {


            if (db.AppStats.Count() != 0)
            {
                if (db.AppStats.ToList().Last().dayOfWeek != DateTime.Now.DayOfWeek)
                {
                    var appStat = new AppStat();
                    increamentVibe(appStat, vibe);
                    db.AppStats.Add(appStat);
                    db.AppStats.ToList().Last().counter += 1;
                }
                else {
                    increamentVibe(db.AppStats.ToList().Last(), vibe);
                    db.AppStats.ToList().Last().counter++;
                }
            }
            else {
                db.AppStats.Add(new AppStat() { counter = 1 });
            }
            db.SaveChanges();
        }

        public static void increamentVibe(AppStat appStat, string vibe) {
            if (vibe.Trim().ToLower() == "chilled")
                appStat.chilledCounter++;
            else if (vibe.Trim().ToLower() == "pub/bar")
                appStat.pubCounter++;
            else if (vibe.Trim().ToLower() == "club")
                appStat.clubCounter++;
            else
                appStat.outdoorCounter++;
        }


        internal static Dictionary<int,string> getIndawoNames(List<Indawo> list)
        {
            var strList = new Dictionary<int,string>();
            foreach (var item in list.OrderBy(x => x.name))
            {
                strList.Add(item.id,item.name);
            }
            return strList;
        }

        internal static string BindSplit(IEnumerable<string> enumerable)
        {
            var str = "";
            foreach (var item in enumerable)
                str += item + ",";
            return str;
        }

        internal static Dictionary<int, string> getEventNames(List<Event> @events)
        {
            var strList = new Dictionary<int, string>();
            foreach (var item in events)
            {
                strList.Add(item.id, item.title);
            }
            return strList;
        }
        internal static Dictionary<int, string> getArtistNames(List<Artist> artists)
        {
            var strList = new Dictionary<int, string>();
            foreach (var item in artists)
            {
                strList.Add(item.id, item.name);
            }
            return strList;
        }



        public static void Shuffle<T>(this List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static List<Indawo> getPlacesWithIn(List<Indawo> indawo,int distance)
        {
            var finalList = new List<Indawo>();
            foreach (var item in indawo)
                if (item.distance <= distance)
                    finalList.Add(item);
            return finalList;
        }

        public static bool IsEmail(string email) {
            if (!email.Contains('@')) {
                return false;
            }
            return true;
        }

        private static double Distance(double lon1,double lat1,double lon2,double lat2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = (lat2 - lat1).ToRadians();
            var dLon = (lon2 - lon1).ToRadians();
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1.ToRadians()) * Math.Cos(lat2.ToRadians()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        internal static object LiekdFromString(string likesLocations,string interestedEvents, 
            List<Indawo> indawoes, List<Event> events, ApplicationDbContext db,double lat, double lon)
        {
            var liked = new List<Indawo>();
            var interested = new List<Event>();
            if(!string.IsNullOrEmpty(likesLocations))
            foreach (var indawoId in likesLocations.Split(',')){
                try{
                    liked.Add(Helper.addDistance(indawoes.First(x => x.id == Convert.ToInt32(indawoId)),lat,lon));
                }
                catch (Exception){}
            }
            if(!string.IsNullOrEmpty(interestedEvents))
            foreach (var id in interestedEvents.Split(',')){
                int outPut;
                if (int.TryParse(id, out outPut))
                {
                    var evnt = db.Events.Find(Convert.ToInt32(id));
                    if (int.TryParse(lat.ToString()[1].ToString(), out outPut) && int.TryParse(lon.ToString()[0].ToString(), out outPut))
                    {
                        var locationLat = Convert.ToDouble(evnt.lat, CultureInfo.InvariantCulture);
                        var locationLon = Convert.ToDouble(evnt.lon, CultureInfo.InvariantCulture);
                        var userLocationLat = Convert.ToDouble(lat, CultureInfo.InvariantCulture);
                        var userLocationLong = Convert.ToDouble(lon, CultureInfo.InvariantCulture);
                        evnt.distance = Math.Round(Helper.distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K'));
                    }
                    var eventArtistIds = db.ArtistEvents.ToList().Where(x => x.eventId == evnt.id);
                    evnt.artists = Helper.getArtists(eventArtistIds, db);
                    evnt.images = db.Images.Where(x => x.eventName.ToLower().Trim() == evnt.title.ToLower().Trim()).ToList();
                    evnt.date = Helper.treatDate(evnt.date);
                    interested.Add(evnt);
                }
                else {
                    continue;
                }
            }
            foreach (var ndawo in liked){
                Helper.prepareLocation(ndawo, db);
            }
            
            return new { liked = liked.Distinct().ToList(), interested = interested.Distinct().ToList() };
        }

        private static Indawo addDistance(Indawo indawo,double lat, double lon)
        {
            var userLocationLat = Convert.ToDouble(lat, CultureInfo.InvariantCulture);
            var userLocationLong = Convert.ToDouble(lon, CultureInfo.InvariantCulture);
            var locationLat = Convert.ToDouble(indawo.lat, CultureInfo.InvariantCulture);
            var locationLon = Convert.ToDouble(indawo.lon, CultureInfo.InvariantCulture);
            var distanceToIndawo = distanceToo(locationLat, locationLon, userLocationLat, userLocationLong, 'K');
            indawo.distance = Math.Round(distanceToIndawo);
            return indawo;
        }

        public static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        public static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static double distanceToo(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        public static void prepareLocation(Indawo item, ApplicationDbContext db) {
            var OpHours = db.OperatingHours.Where(x => x.indawoId == item.id).ToArray();
            item.images = db.Images.Where(x => x.indawoId == item.id).ToList();
            item.events = db.Events.Where(x => x.indawoId == item.id).ToList();
            item.specialInstructions = db.SpecialInstructions.Where(x => x.indawoId == item.id).ToList();
            item.oparatingHours = SortHours(OpHours);
            Helper.makeAllOpHoursToday(item);
            item.open = Helper.assignSatus(item);
            item.closingSoon = Helper.isClosingSoon(item);
            item.openingSoon = Helper.isOpeningSoon(item);
            item.info = Helper.getLocationInfo(item);
            item.openOrClosedInfo = Helper.getClosedStatus(item);
            Helper.getOpratingHoursStr(item);
        }

        public static OperatingHours[] SortHours(OperatingHours[] opHors)
        {
            var days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            List<OperatingHours> final = new List<OperatingHours>();
            List<OperatingHours> tempList = new List<OperatingHours>();
            string dayToday = DateTime.Now.DayOfWeek.ToString(); // today's operating hours
            OperatingHours todayOp = opHors.FirstOrDefault(x => x.day == dayToday);

            if (todayOp == null)
                return opHors;
            else
            {
                final.Add(todayOp);
                var nextDay = getNextDay(dayToday);
                foreach (var item in opHors.Where(x => x.day != dayToday))
                {
                    if (item.day != nextDay)
                    {
                        tempList.Add(item);
                        continue;
                    }
                    nextDay = getNextDay(item.day);
                    final.Add(item);
                }
                foreach (var item in tempList)
                {
                    final.Add(item);
                }
            }
            return final.ToArray();
        }

        public static Token saveAppUserAndToken(ApplicationUser user, ApplicationDbContext db)
        {
            var appUser = new User() { email = user.Email, password = user.PasswordHash };
            var token = createToken(user.Email);
            db.AppUsers.Add(appUser);
            db.Tokens.Add(token);
            db.SaveChanges();
            return token;
        }


        public static Token createToken(string email)
        {
            var tokenString = Guid.NewGuid().ToString();
            var grantDate = DateTime.Now;
            var endDate = grantDate.AddDays(90);
            return new Token(email, tokenString, grantDate, endDate);
        }

        public static string getNextDay(string curDay)
        {
            if (curDay == "Sunday")
                return "Monday";
            var days = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            return days[days.IndexOf(curDay) + 1];
        }

        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }

        public static double ToRadians(this double angle) {
            return (angle * Math.PI) / 180;
        }

        public static int CreateUser(string username)
        {
            var userStore = new UserStore<IdentityUser>();
            var manager = new UserManager<IdentityUser>(userStore);
            if (manager.Users.Where(x =>x.UserName.ToLower() 
            == username.ToLower()).Count() != 0) {
                return -1; // username taken
            }
                var user = new IdentityUser() { UserName = username };
            IdentityResult result = manager.Create(user);

            if (result.Succeeded)
            {
                return 1;// success
            }
            else
            {
                return 0; // failed 
            }

        }

        public static void makeAllOpHoursToday(Indawo indawo) {
            var dateToday = DateTime.Now;
            var nextDay = DateTime.Now.AddDays(1);
            foreach (var item in indawo.oparatingHours)
            {
                if (DateTime.Now.DayOfWeek.ToString().ToLower() == item.day.ToLower()) {

                    item.closingHour = new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, item.closingHour.Hour,
                            item.closingHour.Minute, item.closingHour.Second);

                    var clHr = item.closingHour.ToString("tt");
                    if (clHr == "AM")
                    {
                        item.closingHour = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, item.closingHour.Hour,
                        item.closingHour.Minute, item.closingHour.Second);
                    }
                    item.openingHour = new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, item.openingHour.Hour,
                            item.openingHour.Minute, item.openingHour.Second);
                }
            }
        }

        public static void populateCitites(Indawo indawo){
            var cities = new List<string> { "Johannesburg","Cate Town","Durban","Pretoria"};
            Random rnd = new Random();
            indawo.city = cities.PickRandom();
            
        }

        public static bool assignSatus(Indawo indawo) {
            var dateToday   = DateTime.Now;
            var dayToday    = dateToday.DayOfWeek;
            var opHours     = indawo.oparatingHours.FirstOrDefault(x => x.day.ToLower() 
                                == dayToday.ToString().ToLower());
            if (opHours == null)
                return false;
            else
                return openOrClosed(opHours,indawo);
        }

        internal static void convertDates(List<Event> list)
        {
            foreach (var item in list){
                item.date = treatDate(item.date);
            }
        }

        public static string treatDate(string date)
        {
            var retStr = "";
            var strArr = date.Split(' ');
            for (int i = 0; i < strArr.Length - 1; i++){
                retStr += strArr[i] + " ";
            }
            return retStr.Trim();
        }

        public static bool openOrClosed(OperatingHours opHours,Indawo indawo) {
            if (opHours.openingHour <= DateTime.Now
                && opHours.closingHour >= DateTime.Now)
                return true;
            else
                return false;
        }

        public static void downloadImage(string imageUrl, string downloadPath)
        {
            using (WebClient client = new WebClient())
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                client.DownloadFile(new Uri(downloadPath), imageUrl);
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        internal static List<Artist> getArtists(IEnumerable<ArtistEvent> eventArtistIds,ApplicationDbContext db)
        {
            List<Artist> artists = new List<Artist>();
            foreach (var item in eventArtistIds){
                artists.Add(db.Artists.First(x => x.id == item.artistId));
            }
            return artists;
        }

        private static bool CheckDayBefore(OperatingHours opHours, Indawo indawo)
        {

            var daybefore = getDayBefore(opHours);
            OperatingHours dayBeforeOphour = getDayBeforeOpHour(daybefore,indawo);
            if (dayBeforeOphour != null)
            {
                if (dayBeforeOphour.closingHour.TimeOfDay > DateTime.Now.TimeOfDay)
                {
                    return true;
                }
            }
            return false;
        }

        private static OperatingHours getDayBeforeOpHour(string daybefore, Indawo indawo)
        {
            foreach (var item in indawo.oparatingHours)
            {
                if (item.day.ToLower().Trim() == daybefore) {
                    return item;
                }
            }
            return null;
        }

        private static string  getDayBefore(OperatingHours opHours)
        {
            var dayOfWeek = "monday,tuesday,wednesday,thursday,friday,saturday,sunday".Split(',');
            var prevDay = "";
            for (int i = 0; i < dayOfWeek.Count(); i++)
            {
                if (opHours.day.ToLower().Trim() == dayOfWeek[i]) {
                    if (i == 0)
                    {
                        prevDay = dayOfWeek[6];
                    }
                    else {
                        prevDay = dayOfWeek[i - 1];
                    }
                }
            }
            return prevDay;
        }

        internal static TimeSpan calcTimeLeft(DateTime endDate){
            return   endDate - DateTime.Now;
        }

        public static bool isClosingSoon(Indawo indawo) {
            var now = DateTime.Now;
            var dayToday = now.DayOfWeek;
            var opHours = indawo.oparatingHours.FirstOrDefault(x => x.day.ToLower()
                            == dayToday.ToString().ToLower());
            
            if (opHours != null)
            {
                var closingHours = opHours.closingHour.TimeOfDay;
                var timeNow = DateTime.Now.TimeOfDay;
                var timeLeft = opHours.closingHour.TimeOfDay.Subtract(timeNow);
                if (closingHours.ToString().First() == '0')
                    timeLeft = getTimeLeft(closingHours, now, timeLeft);
                var anHour = new TimeSpan(1, 0, 0);

                if (timeLeft <= anHour && timeLeft > new TimeSpan(0,0,0))
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static bool isOpeningSoon(Indawo indawo)
        {
            var now = DateTime.Now;
            var dayToday = now.DayOfWeek;
            var opHours = indawo.oparatingHours.FirstOrDefault(x => x.day.ToLower()
                            == dayToday.ToString().ToLower());

            if (opHours != null)
            {
                var closingHours = opHours.openingHour.TimeOfDay;
                var timeNow = DateTime.Now.TimeOfDay;
                var timeLeft = opHours.openingHour.TimeOfDay.Subtract(timeNow);
                var anHour = new TimeSpan(1, 0, 0);
                if (timeLeft <= anHour && timeLeft > new TimeSpan(0, 0, 0))
                    return true;
                else
                    return false;
            }
            return false;
        }



        public static TimeSpan getTimeLeft(TimeSpan closingHours, DateTime now, TimeSpan timeLeft) {
            var numAfterZero = closingHours.ToString().ElementAt(1);
            var hoursAftertwelve = Convert.ToInt32(numAfterZero.ToString());
            var timeTilTwelve = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59)
                .TimeOfDay.Subtract(DateTime.Now.TimeOfDay).Duration();
            timeLeft = timeTilTwelve + new TimeSpan(hoursAftertwelve, 0, 0); // get minuts and hours from closing hour
            return timeLeft;
        }
    }


public static class EnumerableExtension
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}