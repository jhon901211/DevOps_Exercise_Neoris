using System;
using Utilities.Response;

namespace Business.Security.Interface
{
    public interface ISecurity
    {
        /// <summary>
        /// Generate Token
        /// </summary>
        /// <param name="secretName"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        ModelResponse<bool> GenerateToken(string secretName, string secretKey);
    }
}
