using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Configuration
{
    public class AppSettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected AppSettings() { }

        /// <summary>
        /// Instancia de AppSettigns 
        /// </summary>
        public static AppSettings Instance { get; set; } = new AppSettings();

        /// <summary>
        /// Get -Set Environment
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Get -Set SecretKey
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        ///  Get -Set SecretName
        /// </summary>
        public string SecretName { get; set; }

        /// <summary>
        /// Get -Set ApiKey
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Get - Set ApplicationJwtKey
        /// </summary>
        public string ApplicationJwtKey { get; set; }

        /// <summary>
        /// Get - Set MinutesExpirationTime
        /// </summary>
        public int MinutesExpirationTime { get; set; }

        /// <summary>
        /// Get - Set ApplicationMethodEncrypt
        /// </summary>
        public string ApplicationMethodEncrypt { get; set; }

        /// <summary>
        /// Get - Set ClaimsPrincipal
        /// </summary>
        public ClaimsIdentity ClaimsPrincipal { get; set; }
    }
}
