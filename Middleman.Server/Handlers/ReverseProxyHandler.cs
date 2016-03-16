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
    public class ReverseProxyHandler : IMiddlemanRequestHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Uri _backendUri;

        public ReverseProxyHandler(string backendUri)
            : this(new Uri(backendUri))
        {
        }

        public ReverseProxyHandler(Uri backendUri)
        {
            _backendUri = backendUri;

            RewriteHost = true;
            AddForwardedForHeader = false;
            RemoveExpectHeader = false;
        }

        public bool RewriteHost { get; set; }
        public bool AddForwardedForHeader { get; set; }
        public bool RemoveExpectHeader { get; set; }

        public async Task<MiddlemanResponse> GetResponseAsync(MiddlemanContext context, MiddlemanRequest request)
        {

            if (request.Headers.AllKeys.Any(h => h.Equals("VsDebuggerCausalityData", StringComparison.InvariantCultureIgnoreCase)))
            {
                request.Headers.Remove("VsDebuggerCausalityData");
            }

            //if (request.Headers.AllKeys.Any(h => h.Equals("SOAPAction", StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    request.Headers.Remove("SOAPAction");
            //}

            if (request.Headers.AllKeys.Any(
                h => h.Equals("Accept-Encoding", StringComparison.InvariantCultureIgnoreCase) && request.Headers[h].Contains("/")))
            {
                string[] parts = request.Headers["Accept-Encoding"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.Contains("/")).ToArray();

                Log.Debug("Fixed bad [Accept-Encoding] header");

                if (parts.Length > 0)
                {
                    request.Headers["Accept-Encoding"] = string.Join(",", parts);
                }
                else
                {
                    request.Headers.Remove("Accept-Encoding");
                }
            }


            if (request.Headers.AllKeys.Any(
    h => h.Equals("Accept-Encoding", StringComparison.InvariantCultureIgnoreCase) && (request.Headers[h].Contains("gzip") || request.Headers[h].Contains("deflate") || request.Headers[h].Contains("sdch"))))
            {
                string acceptEncodingHeader = (request.Headers["Accept-Encoding"] ?? "").Trim();

                Log.Debug("Disabled gzip compression as it is not currently supported.");

                if (acceptEncodingHeader.Length > 0)
                {
                    acceptEncodingHeader = acceptEncodingHeader.Replace("gzip", "gzip;q=0").Replace("deflate", "deflate;q=0").Replace("sdch", "sdch;q=0").Replace(";;", ";");
                    request.Headers["Accept-Encoding"] = acceptEncodingHeader;
                }
                else
                {
                    acceptEncodingHeader = "gzip;q=0,deflate;q=0,sdch;q=0";
                    request.Headers["Accept-Encoding"] = acceptEncodingHeader;
                }
            }


            if (RewriteHost)
            {
                request.Headers["Host"] = _backendUri.Host +
                                          (_backendUri.IsDefaultPort ? string.Empty : ":" + _backendUri.Port);
            }

            if (AddForwardedForHeader)
            {
                SetForwardedForHeader(context, request);
            }

            if (RemoveExpectHeader &&
                request.Headers.AllKeys.Any(
                    h => h.Equals(HttpRequestHeader.Expect.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            {
                request.Headers.Remove(HttpRequestHeader.Expect);
            }

            //if (request.Headers.AllKeys.Any(h => h.Equals(HttpRequestHeader.Range.ToString(), StringComparison.InvariantCultureIgnoreCase))) 
            //    request.Headers.Remove(HttpRequestHeader.Range);

            //if (request.Headers.AllKeys.Any(h => h.Replace("-","").Equals(HttpRequestHeader.IfRange.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            //    request.Headers.Remove(HttpRequestHeader.IfRange);

            //if (request.Headers.AllKeys.Any(h => h.Replace("-", "").Equals(HttpRequestHeader.IfModifiedSince.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            //    request.Headers.Remove(HttpRequestHeader.IfModifiedSince);

            //if (request.Headers.AllKeys.Any(h => h.Replace("-", "").Equals(HttpRequestHeader.IfMatch.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            //    request.Headers.Remove(HttpRequestHeader.IfMatch);

            //if (request.Headers.AllKeys.Any(h => h.Replace("-", "").Equals(HttpRequestHeader.IfNoneMatch.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            //    request.Headers.Remove(HttpRequestHeader.IfNoneMatch);

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

            Log.Debug("{0}: Resolved upstream server to {1} in {2}ms, opening connection",
                context.InboundConnection.RemoteEndPoint, backendEp, sw.Elapsed.TotalMilliseconds);

            if (_backendUri.Scheme != "https")
                await context.OpenOutboundConnectionAsync(backendEp);
            else
                await context.OpenSecureOutboundConnectionAsync(backendEp, _backendUri.Host);

            Log.Debug("{0}: Outbound connection established, sending request",
                context.InboundConnection.RemoteEndPoint);
            sw.Restart();
            await context.OutboundConnection.WriteRequestAsync(request);
            Log.Debug("{0}: Handler sent request in {1}ms", context.InboundConnection.RemoteEndPoint,
                sw.Elapsed.TotalMilliseconds);

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