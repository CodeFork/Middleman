using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Middleman.Server.Connection;
using Middleman.Server.Context;
using Middleman.Server.Handlers;

namespace Middleman.Server.Server
{
    public class SwitchboardServer
    {
        private readonly ISwitchboardRequestHandler _handler;
        private readonly TcpListener _server;

        public SwitchboardServer(IPEndPoint listenEp, ISwitchboardRequestHandler handler)
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
                await inbound.OpenAsync(ct);

                Debug.WriteLine("{0}: Connected", inbound.RemoteEndPoint);

                var context = new SwitchboardContext(inbound);

                HandleSession(context);
            }
        }

        protected virtual Task<InboundConnection> CreateInboundConnection(TcpClient client)
        {
            return Task.FromResult(new InboundConnection(client));
        }

        private async void HandleSession(SwitchboardContext context)
        {
            try
            {
                Debug.WriteLine("{0}: Starting session", context.InboundConnection.RemoteEndPoint);

                do
                {
                    var request = await context.InboundConnection.ReadRequestAsync().ConfigureAwait(false);

                    if (request == null)
                        return;

                    Debug.WriteLine("{0}: Got {1} request for {2}", context.InboundConnection.RemoteEndPoint,
                        request.Method, request.RequestUri);

                    var response = await _handler.GetResponseAsync(context, request).ConfigureAwait(false);
                    Debug.WriteLine("{0}: Got response from handler ({1})", context.InboundConnection.RemoteEndPoint,
                        response.StatusCode);

                    await context.InboundConnection.WriteResponseAsync(response).ConfigureAwait(false);
                    Debug.WriteLine("{0}: Wrote response to client", context.InboundConnection.RemoteEndPoint);

                    if (context.OutboundConnection != null && !context.OutboundConnection.IsConnected)
                        context.Close();
                } while (context.InboundConnection.IsConnected);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("{0}: Error: {1}", context.InboundConnection.RemoteEndPoint, exc.Message);
                context.Close();
                Debug.WriteLine("{0}: Closed context Error: {1}", context.InboundConnection.RemoteEndPoint, exc.Message);
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}