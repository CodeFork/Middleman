using System;
using System.Diagnostics;
using System.Net;
using Middleman.ConsoleHost.Logging;
using Middleman.Server.Handlers;
using Middleman.Server.Server;

namespace Middleman.ConsoleHost
{
    internal class Program
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            // Dump all debug data to the console, coloring it if possible
            Trace.Listeners.Add(new ConsoleLogger());

            var endPoint = new IPEndPoint(IPAddress.Loopback, 8080);
            var handler = new SimpleReverseProxyHandler("http://www.nytimes.com");
            var server = new MiddlemanServer(endPoint, handler);

            server.Start();

            Console.WriteLine("Point your browser at http://{0}", endPoint);

            Console.ReadLine();
        }
    }
}