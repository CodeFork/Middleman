using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMachine;
using Middleman.Server.Connection;
using Middleman.Server.Utils;
using NLog;

namespace Middleman.Server.Request
{
    internal class MiddlemanRequestParser
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<MiddlemanRequest> ParseAsync(InboundConnection conn, Stream stream)
        {
            var del = new ParseDelegate();
            var parser = new HttpParser(del);

            int read;
            var readTotal = 0;
            var buffer = new byte[8192];

            Log.Debug("{0}: RequestParser starting", conn.RemoteEndPoint);

            var requestString = "";

            while (stream != null && (read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                requestString += Encoding.ASCII.GetString(buffer.Where(x => x != 0).ToArray(), 0, read);
                readTotal += read;

                if (parser.Execute(new ArraySegment<byte>(buffer, 0, read)) != read)
                    throw new FormatException("Parse error in request");

                if (del.HeaderComplete)
                    break;
            }

            //conn.MustClose = del.Request.Headers.AllKeys.Any(h => h.Equals("Connection", StringComparison.InvariantCultureIgnoreCase) && del.Request.Headers[h].Equals("Close", StringComparison.InvariantCultureIgnoreCase));

            Log.Debug("{0}: RequestParser read enough ({1} bytes)", conn.RemoteEndPoint, readTotal);
            Log.Info("ORIGINAL REQUEST: " + Environment.NewLine + requestString + Environment.NewLine);

            if (readTotal == 0)
                return null;

            if (!del.HeaderComplete)
                throw new FormatException("Parse error in request");

            var request = del.Request;

            request.ProtocolVersion = new Version(parser.MajorVersion, parser.MinorVersion);
            conn.RequestVersion = request.ProtocolVersion;

            var cl = request.ContentLength;

            if (cl > 0 && stream != null)
            {
                request.RequestBody = del.RequestBodyStart.Count > 0
                    ? new MaxReadStream(new StartAvailableStream(del.RequestBodyStart, stream), cl)
                    : new MaxReadStream(stream, cl);
            }

            return request;
        }

        private sealed class ParseDelegate : IHttpParserHandler
        {
            public readonly MiddlemanRequest Request = new MiddlemanRequest();
            private string _headerName;
            public bool HeaderComplete;
            public ArraySegment<byte> RequestBodyStart;

            void IHttpParserHandler.OnBody(HttpParser parser, ArraySegment<byte> data)
            {
                RequestBodyStart = data;
            }

            void IHttpParserHandler.OnFragment(HttpParser parser, string fragment)
            {
            }

            void IHttpParserHandler.OnHeaderName(HttpParser parser, string name)
            {
                _headerName = name;
            }

            void IHttpParserHandler.OnHeaderValue(HttpParser parser, string value)
            {
                Request.Headers.Add(_headerName, value);
            }

            void IHttpParserHandler.OnHeadersEnd(HttpParser parser)
            {
                HeaderComplete = true;
            }

            void IHttpParserHandler.OnMessageBegin(HttpParser parser)
            {
            }

            void IHttpParserHandler.OnMessageEnd(HttpParser parser)
            {
            }

            void IHttpParserHandler.OnMethod(HttpParser parser, string method)
            {
                Request.Method = method;
            }

            void IHttpParserHandler.OnQueryString(HttpParser parser, string queryString)
            {
            }

            void IHttpParserHandler.OnRequestUri(HttpParser parser, string requestUri)
            {
                Request.RequestUri = requestUri;
            }
        }
    }
}