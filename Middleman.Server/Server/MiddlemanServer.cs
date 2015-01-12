using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Middleman.Server.Connection;
using Middleman.Server.Context;
using Middleman.Server.Handlers;
using NLog;

namespace Middleman.Server.Server
{
    public class MiddlemanServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IMiddlemanRequestHandler _handler;
        private readonly TcpListener _server;

        public MiddlemanServer(IPEndPoint listenEp, IMiddlemanRequestHandler handler)
        {
            _server = new TcpListener(listenEp);
            _handler = handler;
        }

        public void Start()
        {
            _server.Start();
            Run(CancellationToken.None);
        }

        private async void Run(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var client = await _server.AcceptTcpClientAsync();

                var inbound = await CreateInboundConnection(client);
                if (client.Connected && client.Available > 0 && inbound.IsConnected)
                {
                    await inbound.OpenAsync(ct);
                }
                Log.Info("{0}: Connected", inbound.RemoteEndPoint);

                var context = new MiddlemanContext(inbound);

                HandleSession(context);
            }
        }

        protected virtual Task<InboundConnection> CreateInboundConnection(TcpClient client)
        {
            return Task.FromResult(new InboundConnection(client));
        }

        private async void HandleSession(MiddlemanContext context)
        {
            try
            {
                Log.Info("{0}: Starting session", context.InboundConnection.RemoteEndPoint);

                do
                {
                    var request = await context.InboundConnection.ReadRequestAsync().ConfigureAwait(false);

                    if (request == null)
                        return;

                    Log.Info("{0}: Got {1} request for {2}", context.InboundConnection.RemoteEndPoint,
                        request.Method, request.RequestUri);

                    var response = await _handler.GetResponseAsync(context, request).ConfigureAwait(false);
                    Log.Info("{0}: Got response from handler ({1})", context.InboundConnection.RemoteEndPoint,
                        response.StatusCode);

                    await context.InboundConnection.WriteResponseAsync(response).ConfigureAwait(false);
                    Log.Info("{0}: Wrote response to client", context.InboundConnection.RemoteEndPoint);

                    if (context.OutboundConnection != null && !context.OutboundConnection.IsConnected)
                        context.Close();
                } while (context.InboundConnection.IsConnected);
            }
            catch (Exception exc)
            {
                Log.ErrorException(string.Format("{0}: HandleSession Error.", context.InboundConnection.RemoteEndPoint), exc);
                context.Close();
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}