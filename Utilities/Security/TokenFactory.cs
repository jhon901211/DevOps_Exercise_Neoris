namespace Utilities.Security
{
    public class TokenFactory
    {
        private static ITokenManager instance = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected TokenFactory() { }

        /// <summary>
        /// Crea una nueva instancia.
        /// </summary>
        /// <param name="minutesExpirationTime"></param>
        /// <param name="applicationKey"></param>
        /// <param name="applicationMethodEncrypt"></param>
        /// <returns>Instancia interfaz ITokenManager</returns>
        public static ITokenManager CreateManager(int minutesExpirationTime, string applicationKey)
        {
            if (instance == null)
            {
                instance = new TokenManager(minutesExpirationTime, applicationKey);
            }

            return instance;
        }

    }
}
