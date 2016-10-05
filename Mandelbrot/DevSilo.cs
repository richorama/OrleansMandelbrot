using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    class DevSilo : IDisposable
    {
        private static List<OrleansHostWrapper> hostWrappers = new List<OrleansHostWrapper>();
        static bool isPrimary = true;
        static int portAllocation = 50000;

        public DevSilo()
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            var args = new string[] { };
            if (isPrimary)
            {
                isPrimary = false;
                args = new string[] { "primary" };
            }
            else
            {
                args = new string[] { "secondary", (portAllocation++).ToString(), (portAllocation++).ToString() };
            }

            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });
        }

        static void InitSilo(string[] args = null)
        {
            OrleansHostWrapper hostWrapper = null;
            if (args[0] == "primary")
            {
                hostWrapper = new OrleansHostWrapper();
            }
            else
            {
                hostWrapper = new OrleansHostWrapper(int.Parse(args[1]), int.Parse(args[2]));
            }

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
            hostWrappers.Add(hostWrapper);
        }

        public void Dispose()
        {
            foreach (var hostWrapper in hostWrappers)
            {
                if (hostWrapper == null) return;
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }
    }

}