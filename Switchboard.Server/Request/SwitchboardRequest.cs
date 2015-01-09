using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Server.Request
{
    public class SwitchboardRequest
    {
        private static long _requestCounter;

        public SwitchboardRequest()
        {
            Headers = new WebHeaderCollection();
            RequestId = Interlocked.Increment(ref _requestCounter);
        }

        public long RequestId { get; private set; }
        public Version ProtocolVersion { get; set; }
        public string Method { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public string RequestUri { get; set; }
        public Stream RequestBody { get; set; }
        public bool IsRequestBuffered { get; private set; }

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

        public async Task CloseAsync()
        {
            if (ContentLength > 0 && RequestBody != null && RequestBody.CanRead)
            {
                var buf = new byte[8192];

                while ((await RequestBody.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false)) > 0)
                {
                }
            }
        }

        public async Task BufferRequestAsync()
        {
            if (IsRequestBuffered)
                return;

            if (RequestBody == null)
                return;

            var ms = new MemoryStream();

            await RequestBody.CopyToAsync(ms);

            RequestBody = ms;
            ms.Seek(0, SeekOrigin.Begin);

            IsRequestBuffered = true;
        }
    }
}