using Klarna.Common.Models;

namespace Klarna.Common
{
    /// <summary>
    /// Client interface for the API resources
    /// </summary>
    public class Klarna
    {
        /// <summary>
        /// Order Management API
        /// The Order Management API is used for handling an order after the customer has completed the purchase.
        /// It is used for updating, capturing and refunding an order as well as to see the history of events that have
        /// affected this order.
        /// <a href="https://developers.klarna.com/api/#order-management-api">
        ///     https://developers.klarna.com/api/#order-management-api
        /// </a>
        /// </summary>
        public OrderManagementStore OrderManagement { get; }

        /// <summary>
        /// The current API session object
        /// </summary>
        public ApiSession ApiSession { get; }

        /// <summary>
        /// Creates an instance of the Klarna client.
        /// </summary>
        /// <param name="username">
        ///     Consists of your Merchant ID (eid) - a unique number that identifies your e-store, combined with a random string.
        /// </param>
        /// <param name="password">
        ///     a string which is associated with your Merchant ID and is used to authorize use of Klarna's APIs
        /// </param>
        /// <param name="environment">
        ///     The API is accessible through a few different URLS. There are different URLs for testing and for making
        ///     live purchases as well as different URLs for depending on if you are based in Europe or in the U.S.
        /// </param>
        /// <param name="userAgent">The user agent string used when calling the Klarna API</param>
        public Klarna(string username, string password, KlarnaEnvironment environment, string userAgent = "") :
            this(username, password, ApiUrlFromEnvironment(environment), userAgent)
        {
        }

        /// <summary>
        /// Creates an instance of the Klarna client.
        /// </summary>
        /// <param name="credentials">DTO class for Klarna API Credentials</param>
        /// <param name="environment">
        ///     The API is accessible through a few different URLS. There are different URLs for testing and for making
        ///     live purchases as well as different URLs for depending on if you are based in Europe or in the U.S.
        /// </param>
        /// <param name="userAgent">The user agent string used when calling the Klarna API</param>
        public Klarna(ApiCredentials credentials, KlarnaEnvironment environment, string userAgent = "") :
            this(new ApiSession
            {
                ApiUrl = ApiUrlFromEnvironment(environment),
                UserAgent = userAgent,
                Credentials = credentials
            })
        {
        }

        /// <summary>
        /// Creates an instance of the Klarna client.
        /// </summary>
        /// <param name="username">
        ///     Consists of your Merchant ID (eid) - a unique number that identifies your e-store, combined with
        ///     a random string.
        /// </param>
        /// <param name="password">
        ///     a string which is associated with your Merchant ID and is used to authorize use of Klarna's APIs
        /// </param>
        /// <param name="apiUrl">
        ///     The API is accessible through a few different URLS. There are different URLs for testing and for
        ///     making live purchases as well as different URLs for depending on if you are based in Europe or
        ///     in the U.S.
        /// </param>
        /// <param name="userAgent">The user agent string used when calling the Klarna API</param>
        public Klarna(string username, string password, string apiUrl, string userAgent = "") :
            this(new ApiSession
            {
                ApiUrl = apiUrl,
                UserAgent = userAgent,
                Credentials = new ApiCredentials
                {
                    Username = username,
                    Password = password
                }
            })
        {
        }

        /// <summary>
        /// Creates an instance of the Klarna client.
        /// </summary>
        /// <param name="credentials">DTO class for Klarna API Credentials</param>
        /// <param name="apiUrl">
        ///     The API is accessible through a few different URLS. There are different URLs for testing and for
        ///     making live purchases as well as different URLs for depending on if you are based in
        ///     Europe or in the U.S.
        /// </param>
        /// <param name="userAgent">The user agent string used when calling the Klarna API</param>
        public Klarna(ApiCredentials credentials, string apiUrl, string userAgent = "") :
            this(new ApiSession {ApiUrl = apiUrl, UserAgent = userAgent, Credentials = credentials})
        {
        }

        /// <summary>
        /// Creates an instance of the Klarna client.
        /// </summary>
        /// <param name="apiSession">Session representation object for communicating with the Klarna API</param>
        public Klarna(ApiSession apiSession) : this(apiSession, new JsonSerializer())
        {
        }

        /// <summary>
        /// Creates an instance of the Klarna client.
        /// </summary>
        /// <param name="apiSession">Session representation object for communicating with the Klarna API</param>
        /// <param name="jsonSerializer">Instance of a class implementing the IJsonSerializer interface</param>
        public Klarna(ApiSession apiSession, IJsonSerializer jsonSerializer)
        {
            if (string.IsNullOrEmpty(apiSession.UserAgent))
            {
                apiSession.UserAgent = GetDefaultUserAgent();
            }

            OrderManagement = new OrderManagementStore(apiSession, jsonSerializer);
            ApiSession = apiSession;
        }

        /// <summary>
        /// Gets API URL from the environment.
        /// </summary>
        /// <param name="environment">Klarna environment</param>
        /// <returns>Klarna API URL</returns>
        public static string ApiUrlFromEnvironment(KlarnaEnvironment environment)
        {
            switch (environment)
            {
                case KlarnaEnvironment.LiveEurope: return Constants.ProdUrlEurope;
                case KlarnaEnvironment.LiveNorthAmerica: return Constants.ProdUrlNorthAmerica;
                case KlarnaEnvironment.LiveOceania: return Constants.ProdUrlOceania;
                case KlarnaEnvironment.TestingEurope: return Constants.TestUrlEurope;
                case KlarnaEnvironment.TestingNorthAmerica: return Constants.TestUrlNorthAmerica;
                case KlarnaEnvironment.TestingOceania: return Constants.TestUrlOceania;
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Sets and provides default User Agent.
        /// </summary>
        /// <returns>Klarna User-Agent</returns>
        public static string GetDefaultUserAgent()
        {
            return $"Klarna .NET SDK/{Constants.Version} (+Klarna)";
        }
    }
}