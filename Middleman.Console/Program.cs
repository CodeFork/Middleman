using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleman.Server.Server;

namespace Middleman.Console
{
    class Program
    {
        private static ServerManager _serviceManager;

        static void Main(string[] args)
        {
            _serviceManager = ServerManager.Servers();

            Task.Run(() =>
            {
                _serviceManager.StartAll();

                foreach (var s in _serviceManager.AllServers)
                {
                    System.Console.WriteLine("Listening on {0}", s.Port);
                }

                _serviceManager.WaitForConnections();
            });

            System.Console.ReadLine();
        }
    }
}
