using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MiniGoogle.Startup))]
namespace MiniGoogle
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
