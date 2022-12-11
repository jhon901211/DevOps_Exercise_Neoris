namespace Utilities.Security
{
    using Utilities.Secutity;

    public interface ITokenManager
    {
        /// <summary>
        /// Genera el token de autorización
        /// </summary>
        /// <param name="dataToken"></param>
        /// <returns></returns>
        AuthorizationToken GenerateToken(DataToken dataToken);

        /// <summary>
        /// Genera el token de autorización Host to Host
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="environment"></param>
        /// <param name="idApplicationName"></param>
        /// <param name="minutesExpirationTime">Parámetro opcional</param>
        AuthorizationToken GenerateTokenServertoServer(string applicationName, string environment, int idApplication, int minutesExpirationTime = 0);
    }
}
