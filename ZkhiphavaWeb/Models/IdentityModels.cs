using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ZkhiphavaWeb.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("Vawazi", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Indawo> Indawoes { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<AppStat> AppStats { get; set; }
        public DbSet<IndawoStat> IndawoStats { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<ArtistEvent> ArtistEvents { get; set; }
        public DbSet<Request> Requests { get; set; }



        public System.Data.Entity.DbSet<ZkhiphavaWeb.Models.OperatingHours> OperatingHours { get; set; }

        public System.Data.Entity.DbSet<ZkhiphavaWeb.Models.SpecialInstruction> SpecialInstructions { get; set; }
        public System.Data.Entity.DbSet<ZkhiphavaWeb.Models.User> AppUsers { get; set; }
    }
}