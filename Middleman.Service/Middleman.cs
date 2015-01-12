using System.ServiceProcess;
using System.Threading.Tasks;
using Middleman.Server;
using Middleman.Server.Server;

namespace Middleman.Service
{
    public partial class Middleman : ServiceBase
    {
        private ServerManager _serviceManager;

        public Middleman()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _serviceManager = ServerManager.Servers();

            Task.Run(() =>
            {
                _serviceManager.StartAll();
                _serviceManager.WaitForConnections();
            });
        }

        protected override void OnStop()
        {
            _serviceManager.Stop();
            _serviceManager = null;
        }
    }
}