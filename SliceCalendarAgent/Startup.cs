using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SliceCalendarAgent.Startup))]
namespace SliceCalendarAgent
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
