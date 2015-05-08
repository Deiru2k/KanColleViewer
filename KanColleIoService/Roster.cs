using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanColleIoService
{
    class Roster
    {
        /// <summary>
        /// Local item table. Each key corresponds to item ID in global item table. 
        /// </summary>
        private Dictionary<int, int> items = new Dictionary<int, int>();

        /// <summary>
        /// Local ship table.
        /// </summary>
        // private Dictionary<int, ShipData> ships = new Dictionary<int, ShipData>();

        /// <summary>
        /// Maps the selected ID in the local item table to an ID in global table.
        /// </summary>
        /// <param name="localId">ID of the item in local table</param>
        /// <param name="baseId">ID of the item in global table</param>
        public void MapItemId(int localId, int baseId)
        {
            items[localId] = baseId;
        }
    }
}
