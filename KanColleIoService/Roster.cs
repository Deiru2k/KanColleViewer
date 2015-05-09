using System;
using System.Collections.Generic;
using System.Net.Http;
using Grabacr07.KanColleWrapper.Models.Raw;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanColleIoService.Models;

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
        private Dictionary<int, Ship> ships = new Dictionary<int, Ship>();

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
        public async void Initialize()
        {
            // Constructing an URI link. Note that we're passing the fields that we need in the URI.
            string requestUri = "http://api.dev.kancolle.io/v1/roster/{0}?fields=id,origin,baseId,level";
            requestUri = string.Format(requestUri, syncService.UserName);

            try
            {
                // Performing a request to the API
                JToken result = await syncService.APIRequest(HttpMethod.Get, requestUri);

                // Deserializing the JSON response
                Response<Ship[]> shipListResponse =
                    (Response<Ship[]>)JsonConvert.DeserializeObject(result.ToString(), typeof(Response<Ship[]>));
                Ship[] shipList = shipListResponse.data;

                foreach (Ship ship in shipList)
                {
                    // We're only adding ships that were added using KCV.
                    if (ship.origin == "kcv" && ship.id is int)
                        ships.Add((int) ship.id, ship);
                }
            }

            // This should silence the warning that there's no shipgirls in roster
            catch (APIException)
            { }
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
        public async void AddShip(kcsapi_ship2 ship)
        {
            Ship newShip = new Ship {
                id = ship.api_id,
                origin = "kcv",
                baseId = ship.api_ship_id,
                level = ship.api_lv
            };

            ships.Add(ship.api_id, newShip);
            await syncService.APIRequest(HttpMethod.Post, string.Format("roster/{0}", syncService.UserName), newShip);
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
            if (ships.ContainsKey(ship.api_id)) UpdateShip(ship);
            else AddShip(ship);
        }
    }
}
