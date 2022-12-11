
namespace Utilities.Security
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using Utilities.Configuration;
    using Utilities.Functions;
    using Utilities.Secutity;   
    public class TokenManager : ITokenManager
    {
        private readonly int MinutesExpirationTime;
        private readonly string ApplicationKey;

        /// <summary>
        /// Constructor clase token manager
        /// </summary>
        /// <param name="minutesExpirationTime"></param>
        /// <param name="applicationKey"></param>
        public TokenManager(int minutesExpirationTime, string applicationKey)
        {
            MinutesExpirationTime = minutesExpirationTime;
            ApplicationKey = applicationKey;
        }

        public AuthorizationToken GenerateToken(DataToken dataToken)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Authentication, JsonSerializer.SerializeEntity(dataToken.ApiKey))
            };
            var key = Encoding.UTF8.GetBytes(ApplicationKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(MinutesExpirationTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
            AppSettings.Instance.ClaimsPrincipal = tokenDescriptor.Subject;
            return new AuthorizationToken
            {
                TipoToken = "Bearer",
                TiempoExpiracion = DateTime.Now.AddMinutes(MinutesExpirationTime),
                TokenAcceso = new JwtSecurityTokenHandler().WriteToken(token),
                TokenRenovacion = GenerateRefreshToken()
            };
        }

        /// <summary>
        /// Genera código para refrescar el token
        /// </summary>
        /// <returns></returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public AuthorizationToken GenerateTokenServertoServer(string applicationName, string environment, int idApplication, int minutesExpirationTime = 0)
        {
            throw new System.NotImplementedException();
        }
    }
}
