using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using Fiddler;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleViewer.ViewModels;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using Livet;
using Livet.EventListeners;
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
        private KanColleProxy proxy;
        private List<IDisposable> listeners = new List<IDisposable>();

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

        /// <summary>
        /// Performs an asynchronous request to KanColle.io API.
        /// </summary>
        /// <param name="httpMethod">HTTP method to use</param>
        /// <param name="requestUri">URI component of the request</param>
        /// <param name="data">Data to pass</param>
        /// <returns></returns>
        internal async Task<JToken> APIRequest(HttpMethod httpMethod, string requestUri, object data = null)
        {
            // Serializing the provided object and performing the request
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, requestUri);
            if (data != null) requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data));
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            try
            {
                // Retrieving JSON from response and parsing it
                string jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                JToken result = JObject.Parse(jsonResponse);

                // If there are any errors, we throw an API exception
                if ((string)result.SelectToken("status", true) == "error")
                {
                    // If there is a message provided in JSON response, an exception is created
                    // with this message as a parameter.
                    throw new APIException((string)result.SelectToken("message", true));
                }

                // We also throw an exception for 4XX and 5XX status codes
                if (!responseMessage.IsSuccessStatusCode)
                    throw new HttpStatusCodeException(responseMessage.StatusCode);

                return result;
            }
            catch (JsonReaderException)
            {
                // In case we can't parse JSON, we ignore the response body completely
                return null;
            }
        }

        /// <summary>
        /// Reports an exception.
        /// </summary>
        /// <param name="ex">An exception to report</param>
        internal void ReportException(Exception ex)
        {
#if DEBUG
            PluginHost.Instance.GetNotifier().Show(NotifyType.Other, ex.GetType().Name, ex.Message, null);
#endif
        }

        /// <summary>
        /// Performs a login request to the API and saves the received authorization ID.
        /// </summary>
        /// <param name="username">Username on KanColle.io</param>
        /// <param name="password">Password on KanColle.io</param>
        /// <returns>Authentication ID</returns>
        public async Task LogIn(string username, string password)
        {
            if (UserName != null)
                throw new InvalidOperationException("Authentication has already been performed.");

            // Requesting an authentication token
            dynamic result = await APIRequest(HttpMethod.Post, "auth/login", new { username, password });

            // Saving credentials for further authentication
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((string) result.data["$oid"]);
            UserName = username;

            // Telling the roster to fill itself with ship data
            await roster.Initialize();

            // Subscribing to the intercepted API calls
            RegisterAPICalls();
        }

        /// <summary>
        /// Forgets the authorization ID and username used to perform requests.
        /// </summary>
        public void LogOut()
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            UserName = null;

            // Stopping the listening of the API
            UnregisterAPICalls();
        }

        /// <summary>
        /// Registers the objects used by the KCV in the service.
        /// </summary>
        /// <param name="client">Current instance of KanColleClient</param>
        /// <param name="viewModelRoot">Main view model object</param>
        /// <param name="mainWindow">Main window object</param>
        public void RegisterObjects(KanColleClient client, MainWindowViewModel viewModelRoot, Window mainWindow)
        {
            if (this.proxy != null)
                throw new InvalidOperationException("Proxy has already been registered.");

            this.proxy = client.Proxy;

            // This is probably a "kostyl", as they call it in Russian, but I really couldn't come up with a better way to do this.
            UserInterfaceHelpers.AddSettingsTabStart(mainWindow);
            viewModelRoot.CompositeDisposable.Add(new PropertyChangedEventListener(client)
            {
                { () => client.IsStarted, (sender, args) => Task.Run(() => UserInterfaceHelpers.AddSettingsTabMain(mainWindow)) }
            });

            // Called when game starts
            proxy.api_start2.TryParse<kcsapi_start2>().Subscribe(CreateHandler<kcsapi_start2>(ApiMessageHandlers.Start2));
        }

        /// <summary>
        /// Registers API calls intercepted by the proxy.
        /// </summary>
        private void RegisterAPICalls()
        {
            if (listeners.Count > 0)
                throw new InvalidOperationException("API calls have already been registered.");

            // Called when player returns to main menu
            listeners.Add(proxy.api_port.TryParse<kcsapi_port>().Subscribe(CreateHandler<kcsapi_port>(ApiMessageHandlers.Port)));

            // Called when ship drops (whole list of ships is sent)
            listeners.Add(proxy.api_get_member_ship2.TryParse<kcsapi_ship2[]>().Subscribe(CreateHandler<kcsapi_ship2[]>(ApiMessageHandlers.Ship2)));

            // Called when ship equipment is changed / ship itself is remodelled
            listeners.Add(proxy.api_get_member_ship3.TryParse<kcsapi_ship3>().Subscribe(CreateHandler<kcsapi_ship3>(ApiMessageHandlers.Ship3)));

            // Called when item list is initialized
            listeners.Add(proxy.api_get_member_slot_item.TryParse<kcsapi_slotitem[]>().Subscribe(CreateHandler<kcsapi_slotitem[]>(ApiMessageHandlers.SlotItem)));

            // Called when fleet composition is changed
            listeners.Add(proxy.api_req_hensei_change.TryParse<kcsapi_change>().Subscribe(CreateHandler<kcsapi_change>(ApiMessageHandlers.Change)));

            // Called when ship is modernized
            listeners.Add(proxy.api_req_kaisou_powerup.TryParse<kcsapi_powerup>().Subscribe(CreateHandler<kcsapi_powerup>(ApiMessageHandlers.PowerUp)));

            // Called when item is created
            listeners.Add(proxy.api_req_kousyou_createitem.TryParse<kcsapi_createitem>().Subscribe(CreateHandler<kcsapi_createitem>(ApiMessageHandlers.CreateItem)));

            // Called when ship is constructed
            listeners.Add(proxy.api_req_kousyou_getship.TryParse<kcsapi_kdock_getship>().Subscribe(CreateHandler<kcsapi_kdock_getship>(ApiMessageHandlers.GetShip)));
        }

        /// <summary>
        /// Unregisters all the API calls that have been registered before.
        /// </summary>
        private void UnregisterAPICalls()
        {
            foreach (IDisposable listener in listeners)
                listener.Dispose();
            listeners.Clear();
        }

        /// <summary>
        /// Converts an API message handler delegate function to an actual handler that will
        /// efficiently handle any arising exceptions.
        /// </summary>
        /// <typeparam name="T">Model that JSON will be parsed to</typeparam>
        /// <param name="handler">API message handler delegate</param>
        /// <returns>Action that can be used with Subscribe method</returns>
        private Action<SvData<T>> CreateHandler<T>(Action<Roster, T> handler)
        {
            return data =>
            {
                try
                {
                    if (!(handler.Method.Name == "Start2" || roster.HasBaseData()))
                        throw new InvalidOperationException("Base data was not initialized.");

                    // Delegating the actual handling
                    handler(roster, data.Data);
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                }
            };
        }
    }

    /// <summary>
    /// API exception class. This exception is thrown when an error occurs within the API
    /// and there is an error message provided in the response body.
    /// </summary>
    public class APIException : Exception
    {
        public APIException()
        {
        }

        public APIException(string message)
            : base(message)
        {
        }

        public APIException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
    
    /// <summary>
    /// HTTP status code exception.
    /// </summary>
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCodeException(HttpStatusCode httpStatusCode)
        {
            StatusCode = httpStatusCode;
        }

        private HttpStatusCodeException()
        {
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
