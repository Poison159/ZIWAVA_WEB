using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ZkhiphavaWeb.Startup))]
namespace ZkhiphavaWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
