using Mandelbrot.Grains;
using Microsoft.Owin;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public class Controller
    {
        public TaskScheduler TaskScheduler { get; private set; }
        public IProviderRuntime ProviderRuntime { get; private set; }

        public Controller(Router router, TaskScheduler taskScheduler, IProviderRuntime providerRuntime)
        {

            this.TaskScheduler = taskScheduler;
            this.ProviderRuntime = providerRuntime;

            Action<string, Func<IOwinContext, IDictionary<string, string>, Task>> add = router.Add;

            add("/", Index);
            add("/tile/:key", GetTile);
        }

        Task Index(IOwinContext context, IDictionary<string, string> parameters)
        {
            return context.ReturnFile("Index.html", "text/html");
        }

        async Task GetTile(IOwinContext context, IDictionary<string, string> parameters)
        {

            var grainKey = parameters["key"];

            var grain = this.ProviderRuntime.GrainFactory.GetGrain<IMandelbrotGrain>(grainKey);

            var result = await Dispatch(async () =>
            {
                return await grain.Get();
            }) as byte[];

            await context.ReturnBytes(result);
        }

        Task<object> Dispatch(Func<Task<object>> func)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, scheduler: this.TaskScheduler).Result;
        }
    }

}