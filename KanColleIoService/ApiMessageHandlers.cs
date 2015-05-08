using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace KanColleIoService
{
    static class ApiMessageHandlers
    {
        /// <summary>
        /// Handles /kcsapi/api_port/port
        /// </summary>
        public static void Port(Roster roster, kcsapi_port data)
        {
        }

        /// <summary>
        /// /kcsapi/api_get_member/ship2
        /// </summary>
        public static void Ship2(Roster roster, kcsapi_ship2[] data)
        {
        }

        /// <summary>
        /// Handles /kcsapi/api_get_member/ship3
        /// </summary>
        public static void Ship3(Roster roster, kcsapi_ship3 data)
        {
        }

        /// <summary>
        /// Handles /kcsapi/api_get_member/slot_item
        /// </summary>
        public static void SlotItem(Roster roster, kcsapi_slotitem[] data)
        {
            // In normal circumstances, api_id is unique, so there should be no problems
            // if we don't clear the item table before the ID mapping.

            foreach (kcsapi_slotitem item in data)
            {
                roster.MapItemId(item.api_id, item.api_slotitem_id);
            }
        }

        /// <summary>
        /// Handles /kcsapi/api_req_hensei/change
        /// </summary>
        public static void Change(Roster roster, kcsapi_change data)
        {
        }

        /// <summary>
        /// Handles /kcsapi/api_req_kaisou/powerup
        /// </summary>
        public static void PowerUp(Roster roster, kcsapi_powerup data)
        {
        }

        /// <summary>
        /// Handles /kcsapi/api_req_kousyou/createitem
        /// </summary>
        public static void CreateItem(Roster roster, kcsapi_createitem data)
        {
            if (data.api_create_flag == 1)
            {
                roster.MapItemId(data.api_slot_item.api_id, data.api_slot_item.api_slotitem_id);
            }
        }

        /// <summary>
        /// Handles /kcsapi/api_req_kousyou/getship
        /// </summary>
        public static void GetShip(Roster roster, kcsapi_getship data)
        {
        }
    }
}
