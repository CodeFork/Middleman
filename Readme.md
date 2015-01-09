# Switchboard #

Fully asynchronous C# 5 / .NET4.5 HTTP intermediary server. Supports SSL for inbound and outbound connections.

### Potential uses
The lib is still really early in development and it's lacking in several aspects but here's some potential _future_ use cases.

 * Load balancing/reverse proxy
 * Reverse proxy with cache (coupled with a good cache provider)
 * In flight message logging for web services either for temporary debugging or more permanent logging when there's zero or little control over the endpoints.

### Notes/TODO ###

There are CancellationTokens sprinkled throughout but they won't do any smart cancellation as of yet.

No timeout support for connections which never gets around to making a request.

Chunked transfer support is currently limited. It works great for streaming but there's no support for merging chunks into a coherent response. Beware.

## License ##

Licensed under the MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHERLIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.