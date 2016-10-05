using Microsoft.Owin;
using Owin;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public class WebServer
    {
        public WebServer(Router router)
        {
            this.Router = router;
        }

        public Router Router { get; private set; }

        Task HandleRequest(IOwinContext context, Func<Task> func)
        {
            var result = this.Router.Match(context.Request.Path.Value);
            if (null != result)
            {
                return result(context);
            }

            context.Response.StatusCode = 404;
            return Task.FromResult(0);
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use(HandleRequest);
        }
    }

}
