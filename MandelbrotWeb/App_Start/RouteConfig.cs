using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MandelbrotWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Tile",
                url: "{tile}/{x}/{y}/{z}",
                defaults: new { controller = "Tile", action = "Get"  }
            );

            routes.IgnoreRoute("");
        }
    }
}
