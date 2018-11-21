using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Wisky.SkyAdmin.Manage.Startup))]
namespace Wisky.SkyAdmin.Manage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
