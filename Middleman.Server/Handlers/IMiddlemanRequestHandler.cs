using System.Threading.Tasks;
using Middleman.Server.Context;
using Middleman.Server.Request;
using Middleman.Server.Response;

namespace Middleman.Server.Handlers
{
    public interface IMiddlemanRequestHandler
    {
        Task<MiddlemanResponse> GetResponseAsync(MiddlemanContext context, MiddlemanRequest request);
    }
}