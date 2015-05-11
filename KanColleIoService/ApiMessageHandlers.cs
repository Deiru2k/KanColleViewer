using System;
using System.Linq;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace KanColleIoService
{
    static class ApiMessageHandlers
    {
        /// <summary>
        /// Handles /kcsapi/api_start2
        /// </summary>
        public static void Start2(Roster roster, kcsapi_start2 data)
        {
            roster.ProcessBaseData(data);
        }

        /// <summary>
        /// Handles /kcsapi/api_port/port
        /// </summary>
        public static void Port(Roster roster, kcsapi_port data)
        {
            Ship2(roster, data.api_ship);
        }

        /// <summary>
        /// /kcsapi/api_get_member/ship2
        /// </summary>
        public static void Ship2(Roster roster, kcsapi_ship2[] data)
        {
            // "To process" means to try updating the existing ship, and if the ship isn't
            // in the list, to add it there. If there are ships that aren't in kcsapi_ship2 list,
            // but are present in the roster, they should be deleted from there.

            foreach (kcsapi_ship2 ship in data)
                roster.ProcessShip(ship);

            foreach (int id in roster.GetShipIds().Except(data.Select(x => x.api_id)))
                roster.RemoveShip(id);
        }

        /// <summary>
        /// Handles /kcsapi/api_get_member/ship3
        /// </summary>
        public static void Ship3(Roster roster, kcsapi_ship3 data)
        {
            // Instead of delegating the handling to Ship2, we're iterating over the list here.
            // This is because the list of ships in kcsapi_ship3 is partial, and the ships
            // that aren't in it should not be removed from the roster.

            foreach (kcsapi_ship2 ship in data.api_ship_data)
                roster.ProcessShip(ship);
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
            // Not implemented yet in the current version of the API.
        }

        /// <summary>
        /// Handles /kcsapi/api_req_kaisou/powerup
        /// </summary>
        public static void PowerUp(Roster roster, kcsapi_powerup data)
        {
            if (data.api_powerup_flag == 1)
            {
                roster.UpdateShip(data.api_ship);
            }
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
        public static void GetShip(Roster roster, kcsapi_kdock_getship data)
        {
            SlotItem(roster, data.api_slotitem);
            roster.AddShip(data.api_ship);
        }
    }
}
