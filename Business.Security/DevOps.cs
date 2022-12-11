using Business.Security;
using Business.Security.Interface;
using Entities;
using Utilities.Response;

namespace Business
{
    public class DevOps : BaseBusiness, IDevOps
    {
        public DevOps()
        {


        }
        public ModelResponse<bool> SendMessage(RequestMessage requestMessage)
        {
            try
            {
                if (requestMessage== null)
                    return ManagerResponse<bool>.ResponseInternalServerError("please enter values");
                if (string.IsNullOrEmpty(requestMessage.To))
                    return ManagerResponse<bool>.ResponseInternalServerError("please enter destination value");
                if (string.IsNullOrEmpty(requestMessage.Message))
                    return ManagerResponse<bool>.ResponseInternalServerError("please enter message value");
                if (string.IsNullOrEmpty(requestMessage.From))
                    return ManagerResponse<bool>.ResponseInternalServerError("please enter origin value");

                return ManagerResponse<bool>.ResponseOk($"Hello {requestMessage.To} your message will be send");
            }
            catch (System.Exception ex)
            {
                return ManagerResponse<bool>.ResponseInternalServerError(ex.Message);
            }
        }
    }
}
