using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ReadingListPlus.Web.Startup))]
namespace ReadingListPlus.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
