using Entities;
using Utilities.Response;

namespace Business.Security.Interface
{
    public interface IDevOps
    {
        public ModelResponse<bool> SendMessage(RequestMessage requestMessage);
    }
}
