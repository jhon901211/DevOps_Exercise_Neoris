using Business.Security.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Response;

namespace Security.Controllers
{
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ISecurity _managerSecurity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securityInterface"></param>
        public AuthController(ISecurity securityInterface)
        {
            _managerSecurity = securityInterface;
        }

        [Produces("application/json", Type = typeof(bool))]
        [AllowAnonymous]
        [HttpGet("GenerateToken")]
        public IActionResult GenerateToken([FromHeader] string secretName, [FromHeader] string secretKey)
        {
            var response = _managerSecurity.GenerateToken(secretName, secretKey);
            if (response.CodigoRespuesta == System.Net.HttpStatusCode.OK)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Produces("application/json", Type = typeof(bool))]
        [AllowAnonymous]
        [HttpGet("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok(ManagerResponse<bool>.ResponseOk("Health check Success!!!"));
        }
    }
}
