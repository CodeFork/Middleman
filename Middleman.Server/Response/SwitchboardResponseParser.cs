using System;
using System.IO;
using System.Threading.Tasks;
using Middleman.Server.Utils;
using Middleman.Server.Utils.HttpParser;

namespace Middleman.Server.Response
{
    internal class SwitchboardResponseParser
    {
        public async Task<SwitchboardResponse> ParseAsync(Stream stream)
        {
            var del = new ParseDelegate();
            var parser = new HttpResponseParser(del);

            int read;
            var buffer = new byte[8192];

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                parser.Execute(buffer, 0, read);

                if (del.HeaderComplete)
                    break;
            }

            if (!del.HeaderComplete)
                throw new FormatException("Parse error in response");

            var response = del.Response;
            var cl = response.ContentLength;

            if (cl > 0)
            {
                if (del.ResponseBodyStart.Count > 0)
                {
                    response.ResponseBody = new MaxReadStream(new StartAvailableStream(del.ResponseBodyStart, stream),
                        cl);
                }
                else
                {
                    response.ResponseBody = new MaxReadStream(stream, cl);
                }
            }
            else if (response.Headers["Transfer-Encoding"] == "chunked")
            {
                if (response.Headers["Connection"] == "close")
                {
                    response.ResponseBody = del.ResponseBodyStart.Count > 0
                        ? new StartAvailableStream(del.ResponseBodyStart, stream)
                        : stream;
                }
                else
                {
                    response.ResponseBody = del.ResponseBodyStart.Count > 0
                        ? new ChunkedStream(new StartAvailableStream(del.ResponseBodyStart, stream))
                        : new ChunkedStream(stream);
                }
            }

            return response;
        }

        private sealed class ParseDelegate : IHttpResponseHandler
        {
            public bool HeaderComplete;
            public ArraySegment<byte> ResponseBodyStart;
            public readonly SwitchboardResponse Response = new SwitchboardResponse();

            public void OnResponseBegin()
            {
            }

            public void OnStatusLine(Version protocolVersion, int statusCode, string statusDescription)
            {
                Response.ProtocolVersion = protocolVersion;
                Response.StatusCode = statusCode;
                Response.StatusDescription = statusDescription;
            }

            public void OnHeader(string name, string value)
            {
                Response.Headers.Add(name, value);
            }

            public void OnEntityStart()
            {
            }

            public void OnHeadersEnd()
            {
                HeaderComplete = true;
            }

            public void OnEntityData(byte[] buffer, int offset, int count)
            {
                ResponseBodyStart = new ArraySegment<byte>(buffer, offset, count);
            }

            public void OnEntityEnd()
            {
            }

            public void OnResponseEnd()
            {
            }
        }
    }
}