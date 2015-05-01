using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Fiddler;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace KanColleIoService
{
    public class SyncService
    {
        private static SyncService current;

        private SyncService()
        {
        }

        /// <summary>
        /// Gets current instance of a service. As SyncService is a singleton, an instance is
        /// instanciated at first usage of the property and retained thereafter.
        /// </summary>
        public static SyncService Current
        {
            get {
                if (current == null) current = new SyncService();
                return current;
            }
        }

        /// <summary>
        /// Registers API messages that KanColleProxy intercepts.
        /// </summary>
        /// <param name="proxy">The proxy object</param>
        public void RegisterAPIMessages(KanColleProxy proxy)
        {
            // Lambda will be called on selected API message
            // proxy.api_xxx.TryParse<kcsapi_xxx>.Subscribe(x => ...);
        }
    }
}
