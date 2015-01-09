using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Middleman.ConsoleHost.Logging
{
    /// <summary>
    ///     Prints debug messages straight to the console. Will color a message
    ///     based on IP-address and port if it contains one.
    /// </summary>
    public class ConsoleLogger : TraceListener
    {
        private static readonly Regex IpPortRe = new Regex(@"(?:[0-9]{1,3}\.){3}[0-9]{1,3}:\d{1,5}");
        private readonly object _syncRoot = new object();

        public override void Write(string message)
        {
            lock (_syncRoot)
                Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            var m = IpPortRe.Match(message);

            if (m.Success)
            {
                var h = (uint) m.Value.GetHashCode();
                ConsoleColor c;

                do
                {
                    c = (ConsoleColor) (h%16);
                    h++;
                } while (c == ConsoleColor.Black || c == ConsoleColor.Gray);

                lock (_syncRoot)
                {
                    var current = Console.ForegroundColor;
                    Console.ForegroundColor = c;
                    Console.WriteLine(message);
                    Console.ForegroundColor = current;
                }

                return;
            }

            lock (_syncRoot)
            {
                Console.WriteLine(message);
            }
        }
    }
}