using System.Threading.Tasks;
using Switchboard.Server.Context;
using Switchboard.Server.Request;
using Switchboard.Server.Response;

namespace Switchboard.Server.Handlers
{
    public interface ISwitchboardRequestHandler
    {
        Task<SwitchboardResponse> GetResponseAsync(SwitchboardContext context, SwitchboardRequest request);
    }
}