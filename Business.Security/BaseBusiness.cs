using Utilities.Configuration;
using Utilities.Security;

namespace Business.Security
{
    public class BaseBusiness
    {
        /// <summary>
        /// Get - Set TokenManager
        /// </summary>
        public static ITokenManager TokenManager { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
#pragma warning disable S1118 // Utility classes should not have public constructors
        internal BaseBusiness()
#pragma warning restore S1118 // Utility classes should not have public constructors
        {
            TokenManager = TokenFactory.CreateManager(AppSettings.Instance.MinutesExpirationTime, AppSettings.Instance.ApplicationJwtKey);
        }

        
    }
}
