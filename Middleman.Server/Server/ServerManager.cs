using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Middleman.Server.Configuration;
using NLog;
using System.Net;

namespace Middleman.Server.Server
{
    public class ServerManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly object _lock = new object();
        private static ServerManager _instance;
        private readonly List<ManualResetEvent> _semaphores = new List<ManualResetEvent>();
        private readonly List<Server> _servers = new List<Server>();
        private readonly ManualResetEvent _wait = null;

        private ServerManager()
        {

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

        public Server[] AllServers
        {
            get { return _servers.ToArray(); }
        }

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
                    Log.ErrorException("UnhandledException", ex);
                }
            }
            catch
            {
                // meh
            }
        }

        public ServerManager StartAll()
        {
            var config = ConfigurationManager.GetSection("ListenersSection") as ListenerConfigurationSection;

            foreach (ListenerConfiguration lc in config.Listeners)
            {
                var semaphore = new ManualResetEvent(false);
                _semaphores.Add(semaphore);

                ThreadPool.QueueUserWorkItem(StartServerAsync, new {Config = lc, Semaphore = semaphore});

                semaphore.WaitOne();
            }

            return this;
        }

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
            var lc = ((dynamic) state).Config as ListenerConfiguration;
            var sp = ((dynamic) state).Semaphore as ManualResetEvent;

            var s = new Server(lc);

            lock (_lock)
            {
                _servers.Add(s);
                sp.Set();
            }

            s.Start();
        }
    }
}