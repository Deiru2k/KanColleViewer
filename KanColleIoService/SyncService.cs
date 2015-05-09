using System;
using System.Net;
using System.Net.Http;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Web;
using Fiddler;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KanColleIoService
{
    /// <summary>
    /// KanColle.io synchronization service singleton.
    /// </summary>
    public class SyncService
    {
        private static SyncService current;

        private Roster roster;
        private HttpClient httpClient = new HttpClient();

        private SyncService()
        {
            roster = new Roster(this);
            httpClient.BaseAddress = new Uri(Properties.Settings.Default.ApiLink);
        }

        /// <summary>
        /// Gets current instance of a service. As SyncService is a singleton, an instance is
        /// created at the first usage of the property, and retained thereafter.
        /// </summary>
        public static SyncService Current
        {
            get
            {
                if (current == null) current = new SyncService();
                return current;
            }
        }

        /// <summary>
        /// Gets the name of the currently logged user.
        /// </summary>
        public string UserName { get; private set; }

        private async Task<JToken> APIRequest(HttpMethod httpMethod, string requestUri, object data = null)
        {
            // Serializing the provided object and performing the request
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, requestUri);
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data));
            HttpResponseMessage authResponse = await httpClient.SendAsync(requestMessage);

            // Retrieving JSON from response and parsing it
            string jsonResponse = await authResponse.Content.ReadAsStringAsync();
            JToken result = JObject.Parse(jsonResponse);

            // If there are any errors, we throw an API exception
            if ((string) result.SelectToken("status", true) == "error")
            {
                // If there is a message provided in JSON response, an exception is created
                // with this message as a parameter.
                throw new APIException((string) result.SelectToken("message", true));
            }

            return result;
        }

        /// <summary>
        /// Performs a login request to the API and saves the received authorization ID.
        /// </summary>
        /// <param name="username">Username on KanColle.io</param>
        /// <param name="password">Password on KanColle.io</param>
        /// <returns>Authentication ID</returns>
        public async void LogIn(string username, string password)
        {
            dynamic result = await APIRequest(HttpMethod.Post, "auth/login", new { username, password });

            // Saving credentials for further authentication
            httpClient.DefaultRequestHeaders.Authorization = result.data["$oid"];
            UserName = username;

            // Telling the roster to fill itself with ship data
            roster.Initialize();
        }

        /// <summary>
        /// Forgets the authorization ID and username used to perform requests.
        /// </summary>
        public void LogOut()
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            UserName = null;
        }

        /// <summary>
        /// Registers API messages that KanColleProxy intercepts.
        /// </summary>
        /// <param name="proxy">The proxy object</param>
        public void RegisterAPIMessages(KanColleProxy proxy)
        {
            // Called when player returns to main menu
            proxy.api_port.TryParse<kcsapi_port>().Subscribe(data => ApiMessageHandlers.Port(roster, data.Data));

            // Called when ship drops (whole list of ships is sent)
            proxy.api_get_member_ship2.TryParse<kcsapi_ship2[]>().Subscribe(data => ApiMessageHandlers.Ship2(roster, data.Data));

            // Called when ship equipment is changed / ship itself is remodelled
            proxy.api_get_member_ship3.TryParse<kcsapi_ship3>().Subscribe(data => ApiMessageHandlers.Ship3(roster, data.Data));

            // Called when item list is initialized
            proxy.api_get_member_slot_item.TryParse<kcsapi_slotitem[]>().Subscribe(data => ApiMessageHandlers.SlotItem(roster, data.Data));

            // Called when fleet composition is changed
            proxy.api_req_hensei_change.TryParse<kcsapi_change>().Subscribe(data => ApiMessageHandlers.Change(roster, data.Data));

            // Called when ship is modernized
            proxy.api_req_kaisou_powerup.TryParse<kcsapi_powerup>().Subscribe(data => ApiMessageHandlers.PowerUp(roster, data.Data));

            // Called when item is created
            proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(data => ApiMessageHandlers.CreateItem(roster, data.Data));

            // Called when ship is constructed
            proxy.api_req_kousyou_getship.TryParse<kcsapi_getship>().Subscribe(data => ApiMessageHandlers.GetShip(roster, data.Data));
        }

        /// <summary>
        /// API exception class. This exception is thrown when an error occurs within the API
        /// and there is an error message provided in the response body.
        /// </summary>
        class APIException : Exception
        {
            public APIException()
            { }

            public APIException(string message)
                : base(message)
            { }

            public APIException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
    }
}
