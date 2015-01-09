using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Middleman.Server.Response
{
    public class SwitchboardResponse
    {
        private static long _responseCounter;

        public SwitchboardResponse()
        {
            Headers = new WebHeaderCollection();
            ResponseId = Interlocked.Increment(ref _responseCounter);
        }

        public long ResponseId { get; private set; }
        public Version ProtocolVersion { get; set; }
        public string Method { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public Uri RequestUri { get; set; }
        public Stream ResponseBody { get; set; }
        public bool IsResponseBuffered { get; private set; }
        public object StatusCode { get; set; }
        public object StatusDescription { get; set; }

        public int ContentLength
        {
            get
            {
                var clHeader = Headers.Get("Content-Length");

                if (clHeader == null)
                    return 0;

                int cl;

                if (!int.TryParse(clHeader, out cl))
                    return 0;

                return cl;
            }
        }

        public async Task BufferResponseAsync()
        {
            if (IsResponseBuffered)
                return;

            if (ResponseBody == null)
                return;

            var ms = new MemoryStream();

            await ResponseBody.CopyToAsync(ms);

            ResponseBody = ms;
            ms.Seek(0, SeekOrigin.Begin);

            IsResponseBuffered = true;
        }
    }
}