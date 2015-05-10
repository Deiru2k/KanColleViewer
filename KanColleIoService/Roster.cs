using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;
using KanColleIoService.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// Instantiates the roster and links it with the provided service.
        /// </summary>
        /// <param name="syncService"></param>
        public Roster(SyncService syncService)
        {
            this.syncService = syncService;
        }

        private Roster()
        {
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
                    if (ship.origin == "kcv")
                    {
                        try
                        {
                            // When added via KCV, the ship's ID is normally an int, so if there's
                            // any deviation, it's the result of data malformation
                            ships.Add(Convert.ToInt32(ship.id), ship);
                        }

                        // If we can't cast an ID to int, we just ignore the ship
                        catch (InvalidCastException)
                        {
                        }
                    }
                }
            }
            
            // This should silence the warning that there are no shipgirls in the roster.
            catch (APIException)
            {
            }
        }

        public void ProcessBaseData(kcsapi_start2 data)
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
        /// Returns the IDs of all the ships present in the roster.
        /// </summary>
        /// <returns>Enumerable containing IDs of all ships</returns>
        public IEnumerable<int> GetShipIds()
        {
            return ships.Keys;
        }

        /// <summary>
        /// Adds the ship to the list and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public async void AddShip(kcsapi_ship2 ship)
        {
            Ship newShip = new Ship
            {
                id = ship.api_id,
                origin = "kcv",
                baseId = ship.api_ship_id,
                level = ship.api_lv
            };

            try
            {
                JToken result =
                    await syncService.APIRequest(HttpMethod.Post, string.Format("roster/{0}", syncService.UserName), newShip);
                Response<Ship> shipResponse =
                    (Response<Ship>)JsonConvert.DeserializeObject(result.ToString(), typeof(Response<Ship>));

                // For convenience, we're adding the ship object returned by the API and not the locally constructed one.
                ships.Add(Convert.ToInt32(shipResponse.data.id), shipResponse.data);
            }
            catch (Exception ex)
            {
                if (!(ex is APIException || ex is JsonException))
                    throw;
            }
        }

        /// <summary>
        /// Updates the existing ship from the list and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public async void UpdateShip(kcsapi_ship2 ship)
        {
            Ship newShip = new Ship
            {
                id = ship.api_id,
                origin = "kcv",
                baseId = ship.api_ship_id,
                level = ship.api_lv
            };

            // The easiest (maybe not the slickest one) way to compare two data objects
            // is to serialize them and compare two resulting JSON strings.
            string shipJson = JsonConvert.SerializeObject(ships[ship.api_id]);
            string newShipJson = JsonConvert.SerializeObject(newShip);

            if (shipJson != newShipJson)
            {
                string requestUri = string.Format("roster/{0}/{1}", syncService.UserName, ship.api_id);

                try
                {
                    JToken result = await syncService.APIRequest(HttpMethod.Put, requestUri, newShip);
                    Response<Ship> shipResponse =
                        (Response<Ship>)JsonConvert.DeserializeObject(result.ToString(), typeof(Response<Ship>));

                    ships[Convert.ToInt32(shipResponse.data.id)] = shipResponse.data;
                }
                catch (Exception ex)
                {
                    if (!(ex is APIException || ex is JsonException))
                        throw;
                } 
            }
        }

        /// <summary>
        /// For provided ship, chooses what to do with it and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public void ProcessShip(kcsapi_ship2 ship)
        {
            if (ships.ContainsKey(ship.api_id)) UpdateShip(ship);
            else AddShip(ship);
        }

        /// <summary>
        /// Removes the ship with provided id and performs a corresponding request to the API.
        /// </summary>
        /// <param name="id">Ship ID</param>
        public async void RemoveShip(int id)
        {
            string requestUri = string.Format("roster/{0}/{1}", syncService.UserName, id);

            try
            {
                JToken result = await syncService.APIRequest(HttpMethod.Delete, requestUri);

                // If there was no errors, "204 No Content" should be returned.
                if (result == null)
                    ships.Remove(id);
            }

            // Ignore any API exceptions
            catch (APIException)
            {
            }
        }
    }
}
