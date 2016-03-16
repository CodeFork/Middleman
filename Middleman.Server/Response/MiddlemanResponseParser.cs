using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Middleman.Server.Utils;
using Middleman.Server.Utils.HttpParser;
using NLog;

namespace Middleman.Server.Response
{
    internal class MiddlemanResponseParser
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public async Task<MiddlemanResponse> ParseAsync(Stream stream)
        {
            var del = new ParseDelegate();
            var parser = new HttpResponseParser(del);

            int read;
            var buffer = new byte[8192];

            var responseString = "";
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var newBytes = buffer.Where(x => x != 0).ToArray();
                responseString += Encoding.ASCII.GetString(newBytes, 0, Math.Min(newBytes.Length, read));

                parser.Execute(buffer, 0, read);

                if (del.HeaderComplete)
                    break;
            }

            if (responseString.ToLowerInvariant().Contains("content-type: image/"))
            {
                responseString = responseString.Substring(0, responseString.IndexOf(Environment.NewLine + Environment.NewLine)).Trim();
            }
            Log.Info("RESPONSE FROM SERVER: " + Environment.NewLine + responseString.Trim() + Environment.NewLine);

            if (!del.HeaderComplete)
            {
                throw new FormatException("Parse error in response");
            }

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
            //else if ((int)response.StatusCode == 100)
            //{
            //    throw new Exception();
            //}
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
            public readonly MiddlemanResponse Response = new MiddlemanResponse();
            public bool HeaderComplete;
            public ArraySegment<byte> ResponseBodyStart;

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