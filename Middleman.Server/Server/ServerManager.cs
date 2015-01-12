using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Middleman.Server.Configuration;

namespace Middleman.Server.Server
{
    public class ServerManager
    {
        private readonly List<Server> _servers = new List<Server>();
        private static readonly object _lock = new object();
        private List<ManualResetEvent> _semaphores = new List<ManualResetEvent>();
        private static ServerManager _instance = null;

        public static ServerManager Servers()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ServerManager();
                    AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                }
            }

            return _instance;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                var ex = args.ExceptionObject as Exception;
                if (ex != null)
                {

                }
            }
            catch
            {
                // meh
            }
        }

        private ServerManager()
        {
            //ServicePointManager.Expect100Continue = false;
        }

        public int ServerCount
        {
            get
            {
                lock (_lock)
                {
                    return _servers.Count;
                }
            }
        }

        public ServerManager StartAll()
        {
            ListenerConfigurationSection config = ConfigurationManager.GetSection("ListenersSection") as ListenerConfigurationSection;

            foreach (ListenerConfiguration lc in config.Listeners)
            {
                var semaphore = new ManualResetEvent(false);
                _semaphores.Add(semaphore);

                ThreadPool.QueueUserWorkItem(StartServerAsync, new { Config = lc, Semaphore = semaphore });

                semaphore.WaitOne();
            }

            return this;
        }

        private readonly ManualResetEvent _wait = null;
        public void WaitForConnections()
        {
            if (_wait != null)
            {
                _wait.Reset();
            }
        }

        public void Stop()
        {
            if (_wait != null)
            {
                _wait.Set();
                _wait.Dispose();
            }
        }

        private void StartServerAsync(object state)
        {
            var lc = ((dynamic)state).Config as ListenerConfiguration;
            var sp = ((dynamic)state).Semaphore as ManualResetEvent;

            Server s = new Server(lc.DestinationHost, lc.ListenPort, lc.ListenSsl, lc.SslCertName);

            lock (_lock)
            {
                _servers.Add(s);
                sp.Set();
            }

            s.Start();
        }

        public Server[] AllServers
        {
            get { return _servers.ToArray(); }
        }


    }
}
