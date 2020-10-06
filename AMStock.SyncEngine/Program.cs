using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace AMStock.SyncEngine
{
    class Program
    {
        static void Main(string[] args)
        {
#if(!DEBUG)
            var servicesToRun = new ServiceBase[] 
            { 
                new ServiceMain() 
            };
            ServiceBase.Run(servicesToRun);
#else
            var service = new ServiceMain();
            service.DebugStart();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#endif
        }
    }
}
