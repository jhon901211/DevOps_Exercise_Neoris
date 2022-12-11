using Business.Security.Interface;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Utilities.Configuration;
using Utilities.Functions;
using Utilities.Response;

namespace Services.Controllers
{
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class DevOpsController : Controller
    {
        private readonly IDevOps _managerDevOps;

        /// <summary>
        /// 
        /// </summary>
        public DevOpsController(IDevOps devOpsInterface)
        {
            _managerDevOps = devOpsInterface;
        }

        [Produces("application/json", Type = typeof(RequestMessage))]
        //[Authorize]
        [AllowAnonymous]
        [HttpPost]
        public IActionResult DevOps([FromHeader(Name = "X-Parse-REST-API-Key")] string apiKey, [FromHeader(Name = "X-JWT-KWY")] string token, [FromBody] RequestMessage request)
        {
            if (!ValidateToken(token))
                return Unauthorized(ManagerResponse<bool>.ResponseUnauthorized("Invalid token"));
            if (string.IsNullOrEmpty(apiKey))
                return BadRequest(ManagerResponse<bool>.ResponseInternalServerError("please send API Key"));
            if (AppSettings.Instance.ApiKey.ToUpper().Trim() != apiKey.ToUpper().Trim())
                return BadRequest(ManagerResponse<bool>.ResponseInternalServerError("API Key no valid"));
            var response = _managerDevOps.SendMessage(request);
            if (response.CodigoRespuesta == System.Net.HttpStatusCode.OK)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Produces("application/json", Type = typeof(string))]
        [AllowAnonymous]
        [HttpGet("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok(ManagerResponse<bool>.ResponseOk("Health check Success!!!"));
        }

        private bool ValidateToken(string token)
        {
            if (token == null)
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(AppSettings.Instance.ApplicationJwtKey);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var resApiKey = JsonSerializer.DeserializeObject<string>(jwtToken.Claims.First(x => x.Type == ClaimTypes.Authentication).Value);
                if (resApiKey.Equals(AppSettings.Instance.ApiKey))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
