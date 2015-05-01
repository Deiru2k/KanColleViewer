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
            // Called when player returns to main menu
            proxy.api_port.TryParse<kcsapi_port>().Subscribe(data => Console.Beep());

            // Called when ship drops (whole list of ships is sent)
            proxy.api_get_member_ship2.TryParse<kcsapi_ship2[]>().Subscribe(data => Console.Beep());

            // Called when ship equipment is changed / ship itself is remodelled
            proxy.api_get_member_ship3.TryParse<kcsapi_ship3>().Subscribe(data => Console.Beep());

            // Called when item list is initialized
            proxy.api_get_member_slot_item.TryParse<kcsapi_slotitem[]>().Subscribe(data => Console.Beep());

            // Called when fleet composition is changed
            proxy.api_req_hensei_change.TryParse<kcsapi_change>().Subscribe(data => Console.Beep());

            // Called when ship is modernized
            proxy.api_req_kaisou_powerup.TryParse<kcsapi_powerup>().Subscribe(data => Console.Beep());

            // Called when item is created
            proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(data => Console.Beep());

            // Called when item is scrapped
            proxy.api_req_kousyou_destroyitem2.TryParse<kcsapi_destroyitem2>().Subscribe(data => Console.Beep());

            // Called when ship is scrapped
            proxy.api_req_kousyou_destroyship.TryParse<kcsapi_destroyship>().Subscribe(data => Console.Beep());

            // Called when ship is constructed
            proxy.api_req_kousyou_getship.TryParse<kcsapi_getship>().Subscribe(data => Console.Beep());
        }
    }
}
