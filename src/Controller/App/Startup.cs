using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: OwinStartup(typeof(Nimble.Controller.App.Startup))]

namespace Nimble.Controller.App
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            const string root = @".\App\wwwroot";
            var filesystem = new PhysicalFileSystem(root);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = filesystem
            };

            appBuilder.UseFileServer(options);

            //Those who are familier with HttpContext, owinContext is just a brother from another mother.  
            appBuilder.Run((owinContext) =>
            {
                owinContext.Response.ContentType = "text/plain";

                // here comes the performance, everythign in the Katana is Async. Living in the current century.  
                // Let's print our obvious message: :)  
                return owinContext.Response.WriteAsync("Starting Controller...");
            });
        }
    }
}
