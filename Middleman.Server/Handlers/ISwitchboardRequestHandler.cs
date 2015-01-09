using System.Threading.Tasks;
using Middleman.Server.Context;
using Middleman.Server.Request;
using Middleman.Server.Response;

namespace Middleman.Server.Handlers
{
    public interface ISwitchboardRequestHandler
    {
        Task<SwitchboardResponse> GetResponseAsync(SwitchboardContext context, SwitchboardRequest request);
    }
}