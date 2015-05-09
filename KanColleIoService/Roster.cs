using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace KanColleIoService
{
    class Roster
    {
        private SyncService syncService;

        /// <summary>
        /// Local item table. Each key corresponds to item ID in global item table. 
        /// </summary>
        private Dictionary<int, int> items = new Dictionary<int, int>();

        /// <summary>
        /// Local ship table.
        /// </summary>
        private Dictionary<int, object> ships = new Dictionary<int, object>();

        private Roster()
        {
        }

        /// <summary>
        /// Instantiates the roster and links it with the provided service.
        /// </summary>
        /// <param name="syncService"></param>
        public Roster(SyncService syncService)
        {
            this.syncService = syncService;
        }

        /// <summary>
        /// Initializes the roster with data taken from the API.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Maps the selected ID in the local item table to an ID in global table.
        /// </summary>
        /// <param name="localId">ID of the item in local table</param>
        /// <param name="baseId">ID of the item in global table</param>
        public void MapItemId(int localId, int baseId)
        {
            items[localId] = baseId;
        }

        /// <summary>
        /// Adds the ship to the list and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public void AddShip(kcsapi_ship2 ship)
        {
        }

        /// <summary>
        /// Updates the existing ship from the list and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public void UpdateShip(kcsapi_ship2 ship)
        {
        }

        /// <summary>
        /// For provided ship, chooses what to do with it and preforms a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public void ProcessShip(kcsapi_ship2 ship)
        {
        }
    }
}
