using Microsoft.Owin.Hosting;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public class MandelbrotBootstrap : IBootstrapProvider
    {
        public string Name { get; private set; }
       

        public Task Close()
        {
            return TaskDone.Done;
        }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.Name = name;

            var router = new Router();
            new Controller(router, TaskScheduler.Current, providerRuntime);

            var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = config.Properties.ContainsKey("Port") ? int.Parse(config.Properties["Port"]) : 8080,
            };
            WebApp.Start(options, app => new WebServer(router).Configuration(app));

            return TaskDone.Done;
        }
    }
}
