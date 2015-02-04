using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Middleman.Server.Connection;
using NLog;

namespace Middleman.Server.Context
{
    public class MiddlemanContext
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static long _contextCounter;

        public MiddlemanContext(InboundConnection client)
        {
            InboundConnection = client;
            ContextId = Interlocked.Increment(ref _contextCounter);
            Log.Debug("Creating context ({1}) for connection from {0}.", client.RemoteEndPoint, ContextId);
        }

        public long ContextId { get; private set; }
        public InboundConnection InboundConnection { get; private set; }
        public OutboundConnection OutboundConnection { get; private set; }

        public Task<OutboundConnection> OpenSecureOutboundConnectionAsync(IPEndPoint endPoint, string targetHost)
        {
            return OpenOutboundConnectionAsync(endPoint, true, ep => new SecureOutboundConnection(targetHost, ep));
        }

        public Task<OutboundConnection> OpenOutboundConnectionAsync(IPEndPoint endPoint)
        {
            return OpenOutboundConnectionAsync(endPoint, false, ep => new OutboundConnection(ep));
        }

        private async Task<OutboundConnection> OpenOutboundConnectionAsync<T>(IPEndPoint endPoint, bool secure,
            Func<IPEndPoint, T> connectionFactory) where T : OutboundConnection
        {
            if (OutboundConnection != null)
            {
                if (!OutboundConnection.RemoteEndPoint.Equals(endPoint))
                {
                    Log.Debug("{0}: Current outbound connection is for {1}, can't reuse for {2}",
                        InboundConnection.RemoteEndPoint, OutboundConnection.RemoteEndPoint, endPoint);
                    OutboundConnection.Close();
                    OutboundConnection = null;
                }
                else if (OutboundConnection.IsSecure != secure)
                {
                    Log.Debug("{0}: Current outbound connection {0} secure, can't reuse",
                        InboundConnection.RemoteEndPoint);
                    OutboundConnection.Close();
                    OutboundConnection = null;
                }
                else
                {
                    if (OutboundConnection.IsConnected)
                    {
                        Log.Debug("{0}: Reusing outbound connection to {1}", InboundConnection.RemoteEndPoint,
                            OutboundConnection.RemoteEndPoint);
                        return OutboundConnection;
                    }
                    Log.Debug("{0}: Detected stale outbound connection, recreating",
                        InboundConnection.RemoteEndPoint);
                    OutboundConnection.Close();
                    OutboundConnection = null;
                }
            }

            var conn = connectionFactory(endPoint);

            await conn.OpenAsync().ConfigureAwait(false);

            Log.Debug("{0}: Outbound connection to {1} established", InboundConnection.RemoteEndPoint,
                conn.RemoteEndPoint);

            OutboundConnection = conn;

            return conn;
        }

        public async Task<OutboundConnection> OpenOutboundConnectionAsync(Task<OutboundConnection> openTask)
        {
            var conn = await openTask.ConfigureAwait(false);

            OutboundConnection = conn;

            return conn;
        }

        internal void Close()
        {
            if (InboundConnection.IsConnected)
                InboundConnection.Close();

            if (OutboundConnection != null && OutboundConnection.IsConnected)
                OutboundConnection.Close();
        }

        internal void Dispose()
        {
            Close();
        }
    }
}