using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Middleman.Server.Context;
using Middleman.Server.Request;
using Middleman.Server.Response;
using NLog;

namespace Middleman.Server.Handlers
{
    /// <summary>
    ///     Sample implementation of a reverse proxy. Streams requests and responses (no buffering).
    ///     No support for location header rewriting.
    /// </summary>
    public class SimpleReverseProxyHandler : IMiddlemanRequestHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Uri _backendUri;

        public SimpleReverseProxyHandler(string backendUri)
            : this(new Uri(backendUri))
        {
        }

        public SimpleReverseProxyHandler(Uri backendUri)
        {
            _backendUri = backendUri;

            RewriteHost = true;
            AddForwardedForHeader = false;
            RemoveExpectHeader = true;
        }

        public bool RewriteHost { get; set; }
        public bool AddForwardedForHeader { get; set; }
        public bool RemoveExpectHeader { get; set; }

        public async Task<MiddlemanResponse> GetResponseAsync(MiddlemanContext context, MiddlemanRequest request)
        {
            Log.Info("New connection from {0}.", context.InboundConnection.RemoteEndPoint);

            if (RewriteHost)
                request.Headers["Host"] = _backendUri.Host +
                                          (_backendUri.IsDefaultPort ? string.Empty : ":" + _backendUri.Port);

            if (AddForwardedForHeader)
                SetForwardedForHeader(context, request);

            if (RemoveExpectHeader &&
                request.Headers.AllKeys.Any(
                    h => h.Equals(HttpRequestHeader.Expect.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                request.Headers.Remove(HttpRequestHeader.Expect);


            var headers = request.Headers.ToString();

            Log.Info(headers);

            var sw = Stopwatch.StartNew();

            IPAddress ip;

            if (_backendUri.HostNameType == UriHostNameType.IPv4)
            {
                ip = IPAddress.Parse(_backendUri.Host);
            }
            else
            {
                var ipAddresses = await Dns.GetHostAddressesAsync(_backendUri.Host);
                ip = ipAddresses.FirstOrDefault(o => !o.ToString().Contains(":"));
            }

            var backendEp = new IPEndPoint(ip, _backendUri.Port);

            Log.Info("{0}: Resolved upstream server to {1} in {2}ms, opening connection",
                context.InboundConnection.RemoteEndPoint, backendEp, sw.Elapsed.TotalMilliseconds);

            if (_backendUri.Scheme != "https")
                await context.OpenOutboundConnectionAsync(backendEp);
            else
                await context.OpenSecureOutboundConnectionAsync(backendEp, _backendUri.Host);

            Log.Info("{0}: Outbound connection established, sending request",
                context.InboundConnection.RemoteEndPoint);
            sw.Restart();
            await context.OutboundConnection.WriteRequestAsync(request);
            Log.Info("{0}: Handler sent request in {1}ms", context.InboundConnection.RemoteEndPoint,
                sw.Elapsed.TotalMilliseconds);

            Log.Info("New connection from {0} to {1}.", context.InboundConnection.RemoteEndPoint, context.OutboundConnection.RemoteEndPoint);

            var response = await context.OutboundConnection.ReadResponseAsync();

            return response;
        }

        private void SetForwardedForHeader(MiddlemanContext context, MiddlemanRequest request)
        {
            var remoteAddress = context.InboundConnection.RemoteEndPoint.Address.ToString();
            var currentForwardedFor = request.Headers["X-Forwarded-For"];

            request.Headers["X-Forwarded-For"] = string.IsNullOrEmpty(currentForwardedFor)
                ? remoteAddress
                : currentForwardedFor + ", " + remoteAddress;
        }
    }
}