using System.ServiceProcess;

namespace Middleman.Service
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[]
            {
                new Middleman()
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}