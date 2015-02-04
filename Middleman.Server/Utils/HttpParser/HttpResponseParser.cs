using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Middleman.Server.Utils.HttpParser
{
    /// <summary>
    ///     Simple and naive http response parser. No support for chunked transfers or
    ///     line folding in headers. Should probably not be used to read responses from non-friendly
    ///     servers yet.
    ///     TODO:
    ///     * Proper chunked transfer support
    ///     * Proper support for RFC tokens, adhere to the BNF
    ///     * Hardening against maliciously crafted responses.
    ///     *
    /// </summary>
    public class HttpResponseParser
    {
        private static readonly Regex StatusLineRegex =
            new Regex(@"^HTTP/(?<version>\d\.\d) (?<statusCode>\d{3}) (?<statusDescription>.*)");

        private readonly IHttpResponseHandler _handler;
        private readonly byte[] _parseBuffer;
        private bool _chunkedTransfer;
        private int _contentLength = -1;
        private int _entityDataWritten;
        private bool _hasEntityData;
        private bool _hasStarted;
        private bool _inEntityData;
        private bool _inHeaders;
        private bool _isCompleted;
        private int _parseBufferWritten;

        public HttpResponseParser(IHttpResponseHandler handler)
        {
            _handler = handler;
            _parseBuffer = new byte[64*1024];
        }

        public void Execute(byte[] buffer, int offset, int count)
        {
            if (_isCompleted)
                throw new InvalidOperationException("Parser is done");

            if (!_hasStarted)
            {
                _inHeaders = true;
                _hasStarted = true;

                _handler.OnResponseBegin();
            }

            if (!_inHeaders)
            {
                if (!_hasEntityData)
                {
                    _isCompleted = true;
                    _handler.OnResponseEnd();
                    return;
                }

                if (!_inEntityData)
                {
                    _inEntityData = true;
                    _handler.OnEntityStart();
                }

                if (count > 0)
                {
                    _handler.OnEntityData(buffer, offset, count);
                    _entityDataWritten += count;
                }

                if (count == 0 || _entityDataWritten == _contentLength)
                {
                    _inEntityData = false;
                    _isCompleted = true;
                    _handler.OnEntityEnd();
                    _handler.OnResponseEnd();
                }

                return;
            }

            var bufferLeft = _parseBuffer.Length - _parseBufferWritten;

            if (bufferLeft <= 0)
                throw new FormatException("Response headers exceeded maximum allowed length");

            if (count > bufferLeft)
            {
                Execute(buffer, offset, bufferLeft);
                Execute(buffer, offset + bufferLeft, count - bufferLeft);

                return;
            }

            Array.Copy(buffer, offset, _parseBuffer, _parseBufferWritten, count);
            _parseBufferWritten += count;

            var endOfHeaders = IndexOf(_parseBuffer, 0, _parseBufferWritten, 13, 10, 13, 10);

            if (endOfHeaders >= 0)
            {
                ParseHeaders(_parseBuffer, 0, endOfHeaders + 4);

                _inHeaders = false;

                if (endOfHeaders + 4 < _parseBufferWritten)
                    Execute(_parseBuffer, endOfHeaders + 4, _parseBufferWritten - (endOfHeaders + 4));
                else
                {
                    if (!_hasEntityData)
                    {
                        _isCompleted = true;
                        _handler.OnResponseEnd();
                    }
                }
            }
        }

        private void ParseHeaders(byte[] buffer, int offset, int count)
        {
            using (var ms = new MemoryStream(buffer, offset, count))
            using (var sr = new StreamReader(ms, Encoding.ASCII))
            {
                ParseStatusLine(sr.ReadLine());

                string line;

                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                    ParseHeaderLine(line);

                _handler.OnHeadersEnd();

                _hasEntityData = _contentLength > 0 || _chunkedTransfer;
            }
        }

        private void ParseStatusLine(string line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            var m = StatusLineRegex.Match(line);

            if (!m.Success)
                throw new FormatException("Malformed status line");

            var version = m.Groups["version"].Value;

            if (version != "1.1" && version != "1.0")
                throw new FormatException("Unknown http version");

            var statusCode = int.Parse(m.Groups["statusCode"].Value);
            var statusDescription = m.Groups["statusDescription"].Value;

            _handler.OnStatusLine(new Version(version), statusCode, statusDescription);
        }

        private void ParseHeaderLine(string line)
        {
            var parts = line.Split(new[] {':'}, 2);

            if (parts.Length != 2)
                throw new FormatException("Malformed header line");

            parts[1] = parts[1].Trim();

            if (parts[0] == "Content-Length")
            {
                int cl;
                if (int.TryParse(parts[1].Trim(), out cl))
                    _contentLength = cl;
            }
            else if (parts[0] == "Transfer-Encoding")
            {
                if (parts[1] == "chunked")
                    _chunkedTransfer = true;
            }

            _handler.OnHeader(parts[0], parts[1]);
        }

        private int IndexOf(byte[] buffer, int offset, int count, params byte[] elements)
        {
            for (var i = offset; i < offset + count; i++)
            {
                var j = 0;
                for (; j < elements.Length && i + j < offset + count && buffer[i + j] == elements[j]; j++)
                {
                }

                if (j == elements.Length)
                    return i;
            }

            return -1;
        }
    }
}