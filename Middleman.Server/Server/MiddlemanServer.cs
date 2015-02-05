using System;
using System.Diagnostics;
using System.Linq;
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
                //Log.Info("{0} [[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[", _server.LocalEndpoint);

                var client = await _server.AcceptTcpClientAsync();

                //Log.Info("{0}: ...................................................", client.Client.RemoteEndPoint);
                
                var inbound = await CreateInboundConnection(client);
                //if (client.Connected && client.Available > 0 && inbound.IsConnected)
                //{
                    Log.Debug("{0}: Connected", inbound.RemoteEndPoint);
                    await inbound.OpenAsync(ct);
                //}

                var context = new MiddlemanContext(inbound);

                HandleSession(context);

                //Log.Debug("{0}: Connected", inbound.RemoteEndPoint);
                //Log.Info("]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]");
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
                Log.Info("{0}:{1}:{2} >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", context.ContextId, context.InboundConnection.RemoteEndPoint.Address.ToString(), context.InboundConnection.RemoteEndPoint.Port.ToString());
                Log.Debug("{0}: Starting session", context.InboundConnection.RemoteEndPoint);

                int count = 1;
                do
                {
                    if (count > 1)
                    {
                        //Log.Info("{0}:{1}:{2} >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", context.ContextId, context.InboundConnection.RemoteEndPoint.Address.ToString(), context.InboundConnection.RemoteEndPoint.Port.ToString());
                        Log.Info("{0}:{1}:{2} connection alive <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", context.ContextId, context.InboundConnection.RemoteEndPoint.Address.ToString(), context.InboundConnection.RemoteEndPoint.Port.ToString());
                    }

                    var request = await context.InboundConnection.ReadRequestAsync().ConfigureAwait(true);

                    if (request != null)
                    {

                        Log.Info("{0}: Got {1} request {3} for {2}", context.InboundConnection.RemoteEndPoint,
                            request.Method, request.RequestUri, count);

                        var response = await _handler.GetResponseAsync(context, request).ConfigureAwait(false);
                        Log.Info("{0}: Got response from handler ({1})", context.InboundConnection.RemoteEndPoint,
                            response.StatusCode);

                        await context.InboundConnection.WriteResponseAsync(response).ConfigureAwait(false);
                        Log.Info("{0}: Wrote response to client", context.InboundConnection.RemoteEndPoint);

                        //bool closeConnection = response.Headers.AllKeys.Any(h => h.Equals("Connection", StringComparison.InvariantCultureIgnoreCase) && response.Headers[h].Equals("Close", StringComparison.InvariantCultureIgnoreCase));
                        if (context.InboundConnection.MustClose || (context.OutboundConnection != null && !context.OutboundConnection.IsConnected))
                        //if ((context.OutboundConnection != null && !context.OutboundConnection.IsConnected))
                        {
                            //if (context.InboundConnection != null)
                            //{
                            //    context.InboundConnection.Close();
                            //}

                            Log.Info("Closing context!");
                            context.Close();
                            //context.Dispose();
                        }

                        count++;
                    }
                    else
                    {
                        Log.Info("Closing context!");
                        context.Close();
                    }
                } while (context.InboundConnection.IsConnected);

                Log.Info("{0}:{1}:{2} connection closed <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", context.ContextId, context.InboundConnection.RemoteEndPoint.Address.ToString(), context.InboundConnection.RemoteEndPoint.Port.ToString());
            }
            catch (Exception exc)
            {
                Log.ErrorException(string.Format("{0}: HandleSession Error.", context.InboundConnection.RemoteEndPoint), exc);
                context.Close();
            }
            finally
            {
                Log.Info("Closing context!");
            
                context.Close();
                //context.Dispose();
            }
        }
    }
}