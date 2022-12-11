namespace Business.Security
{
    using Business.Security.Interface;
    using System;
    using Utilities.Configuration;
    using Utilities.Response;
    using Utilities.Security;

    public class Authentication : BaseBusiness, ISecurity
    {
        public ModelResponse<bool> GenerateToken(string secretName, string secretKey)
        {
            try
            {
                if (string.IsNullOrEmpty(secretName))
                    return ManagerResponse<bool>.ResponseInternalServerError("please enter secret name");
                if (string.IsNullOrEmpty(secretKey))
                    return ManagerResponse<bool>.ResponseInternalServerError("please enter secret key");
                if (secretKey.Equals(AppSettings.Instance.SecretKey) && secretKey.Equals(AppSettings.Instance.SecretName))
                {
                    DataToken token = new()
                    {
                        ApiKey = AppSettings.Instance.ApiKey
                    };
                    return ManagerResponse<bool>.ResponseOk("Success", TokenManager.GenerateToken(token));
                } else
                {
                    return ManagerResponse<bool>.ResponseInternalServerError("failded secrets values :(");
                }
            }
            catch (Exception ex)
            {
                return ManagerResponse<bool>.ResponseInternalServerError(ex.Message);
            }

        }
    }
}
