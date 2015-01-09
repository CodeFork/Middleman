using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Middleman.Server.Context;
using Middleman.Server.Handlers;
using Middleman.Server.Request;
using Middleman.Server.Response;

namespace Middleman.ConsoleHost
{
    /// <summary>
    ///     Sample implementation of a reverse proxy. Streams requests and responses (no buffering).
    ///     No support for location header rewriting.
    /// </summary>
    public class SimpleReverseProxyHandler : ISwitchboardRequestHandler
    {
        private readonly Uri _backendUri;

        public SimpleReverseProxyHandler(string backendUri)
            : this(new Uri(backendUri))
        {
        }

        public SimpleReverseProxyHandler(Uri backendUri)
        {
            _backendUri = backendUri;

            RewriteHost = true;
            AddForwardedForHeader = true;
        }

        public bool RewriteHost { get; set; }
        public bool AddForwardedForHeader { get; set; }

        public async Task<SwitchboardResponse> GetResponseAsync(SwitchboardContext context, SwitchboardRequest request)
        {
            if (RewriteHost)
                request.Headers["Host"] = _backendUri.Host +
                                          (_backendUri.IsDefaultPort ? string.Empty : ":" + _backendUri.Port);

            if (AddForwardedForHeader)
                SetForwardedForHeader(context, request);

            var sw = Stopwatch.StartNew();

            IPAddress ip;

            if (_backendUri.HostNameType == UriHostNameType.IPv4)
            {
                ip = IPAddress.Parse(_backendUri.Host);
            }
            else
            {
                var ipAddresses = await Dns.GetHostAddressesAsync(_backendUri.Host);
                ip = ipAddresses[0];
            }

            var backendEp = new IPEndPoint(ip, _backendUri.Port);

            Debug.WriteLine("{0}: Resolved upstream server to {1} in {2}ms, opening connection",
                context.InboundConnection.RemoteEndPoint, backendEp, sw.Elapsed.TotalMilliseconds);

            if (_backendUri.Scheme != "https")
                await context.OpenOutboundConnectionAsync(backendEp);
            else
                await context.OpenSecureOutboundConnectionAsync(backendEp, _backendUri.Host);

            Debug.WriteLine("{0}: Outbound connection established, sending request",
                context.InboundConnection.RemoteEndPoint);
            sw.Restart();
            await context.OutboundConnection.WriteRequestAsync(request);
            Debug.WriteLine("{0}: Handler sent request in {1}ms", context.InboundConnection.RemoteEndPoint,
                sw.Elapsed.TotalMilliseconds);

            var response = await context.OutboundConnection.ReadResponseAsync();

            return response;
        }

        private void SetForwardedForHeader(SwitchboardContext context, SwitchboardRequest request)
        {
            var remoteAddress = context.InboundConnection.RemoteEndPoint.Address.ToString();
            var currentForwardedFor = request.Headers["X-Forwarded-For"];

            request.Headers["X-Forwarded-For"] = string.IsNullOrEmpty(currentForwardedFor)
                ? remoteAddress
                : currentForwardedFor + ", " + remoteAddress;
        }
    }
}