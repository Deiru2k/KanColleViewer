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
    internal class Roster
    {
        private SyncService syncService;

        /// <summary>
        /// Global ship table.
        /// </summary>
        private Dictionary<int, kcsapi_mst_ship> globalShips = null;

        /// <summary>
        /// Global item table.
        /// </summary>
        private Dictionary<int, kcsapi_mst_slotitem> globalItems = null;

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
        /// Constructs a ship entry.
        /// </summary>
        /// <param name="ship">API raw ship data</param>
        /// <returns>KanColle.io JSON-serializable ship object</returns>
        public Ship ConstructShip(kcsapi_ship2 ship)
        {
            // Translating local equipment IDs to global ones
            int[] equipment = ship.api_slot.Where(x => x >= 0).Select(x => items[x]).ToArray();

            // Calculating base (without equipment; with modernization) stats of a ship
            Stats stats = new Stats
            {
                hp = ship.api_maxhp,
                firepower = ship.api_karyoku[0] - equipment.Select(x => globalItems[x].api_houg).Sum(),
                armor = ship.api_soukou[0] - equipment.Select(x => globalItems[x].api_souk).Sum(),
                torpedo = ship.api_raisou[0] - equipment.Select(x => globalItems[x].api_raig).Sum(),
                evasion = ship.api_kaihi[0] - equipment.Select(x => globalItems[x].api_houk).Sum(),
                aa = ship.api_taiku[0] - equipment.Select(x => globalItems[x].api_tyku).Sum(),
                aircraft = ship.api_onslot.Sum(),
                asw = ship.api_taisen[0] - equipment.Select(x => globalItems[x].api_tais).Sum(),
                speed = globalShips[ship.api_ship_id].api_soku > 5 ? "fast" : "slow",
                los = ship.api_sakuteki[0] - equipment.Select(x => globalItems[x].api_saku).Sum(),
                range = "short;medium;long;very long".Split(';').ElementAtOrDefault(globalShips[ship.api_ship_id].api_leng - 1), 
                luck = ship.api_lucky[0] - equipment.Select(x => globalItems[x].api_luck).Sum()
            };

            // Constructing a new ship
            return new Ship
            {
                id = ship.api_id,
                origin = "kcv",
                baseId = ship.api_ship_id,
                level = ship.api_lv,
                equipment = equipment,
                stats = stats
            };
        }

        /// <summary>
        /// Initializes the roster with data taken from the API.
        /// </summary>
        public async Task Initialize()
        {
            // Constructing an URI link. Note that we're passing the fields that we need in the URI.
            string requestUri = "roster/{0}?page=all&fields=id,origin,baseId,level,equipment,stats";
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
            catch (APIException ex)
            {
                syncService.ReportException(ex);
            }
        }

        /// <summary>
        /// Processes the base data sent by KanColle API when the game starts.
        /// </summary>
        /// <param name="data">API master data object</param>
        public void ProcessBaseData(kcsapi_start2 data)
        {
            // Saving ship data (used as reference for static stats, i.e. speed)
            globalShips = new Dictionary<int, kcsapi_mst_ship>();
            foreach (kcsapi_mst_ship ship in data.api_mst_ship)
                globalShips[ship.api_id] = ship;

            // Saving item data (used as reference when calculating modernizable stats)
            globalItems = new Dictionary<int, kcsapi_mst_slotitem>();
            foreach (kcsapi_mst_slotitem item in data.api_mst_slotitem)
                globalItems[item.api_id] = item;
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
        public List<int> GetShipIds()
        {
            return ships.Keys.ToList();
        }

        /// <summary>
        /// Adds the ship to the list and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public async Task AddShip(kcsapi_ship2 ship)
        {
            Ship newShip = ConstructShip(ship);

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
                else syncService.ReportException(ex);
            }
        }

        /// <summary>
        /// Updates the existing ship from the list and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public async Task UpdateShip(kcsapi_ship2 ship)
        {
            Ship newShip = ConstructShip(ship);

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
                    else syncService.ReportException(ex);
                } 
            }
        }

        /// <summary>
        /// For provided ship, chooses what to do with it and performs a corresponding request to the API.
        /// </summary>
        /// <param name="ship">KanColle API ship data</param>
        public async Task ProcessShip(kcsapi_ship2 ship)
        {
            if (ships.ContainsKey(ship.api_id)) await UpdateShip(ship);
            else await AddShip(ship);
        }

        /// <summary>
        /// Removes the ship with provided id and performs a corresponding request to the API.
        /// </summary>
        /// <param name="id">Ship ID</param>
        public async Task RemoveShip(int id)
        {
            string requestUri = string.Format("roster/{0}/{1}", syncService.UserName, id);

            try
            {
                JToken result = await syncService.APIRequest(HttpMethod.Delete, requestUri);
                ships.Remove(id);
            }

            // Ignore any API exceptions
            catch (APIException ex)
            {
                syncService.ReportException(ex);
            }
        }

        /// <summary>
        /// Checks if base data was initialized.
        /// </summary>
        /// <returns>True if both of the global tables are filled with data</returns>
        public bool HasBaseData()
        {
            return (globalShips != null) && (globalItems != null);
        }
    }
}
