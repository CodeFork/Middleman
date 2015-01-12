using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Services.Protocols;
using IISExpressAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Middleman.Server.Handlers;
using Middleman.Server;
using Middleman.Server.Server;
using Middleman.Tests.test.asmx;
using TraceLevel = IISExpressAutomation.TraceLevel;

namespace Middleman.Tests
{
    [TestClass]
    public class WebServiceTests
    {
        private static IISExpress _iisExpress = null;

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
            {
                return true;
            };

            var iexps = Process.GetProcessesByName("IISExpress");
            foreach (var iexp in iexps)
            {
                iexp.Kill();
            }

            var projDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.FullName;
            var webroot = Path.Combine(projDir, "Websites");

            _iisExpress = new IISExpress(new Parameters
            {
                Path = webroot,
                Port = 56123,
                Systray = false
            });
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (_iisExpress != null)
            {
                _iisExpress.Dispose();
            }

            var iexps = Process.GetProcessesByName("IISExpress");
            foreach (var iexp in iexps)
            {
                iexp.Kill();
            }
        }

        [TestMethod]
        public void TestHttpAsmx()
        {
            var sm = ServerManager.Servers().StartAll();

            foreach (var s in sm.AllServers)
            {
                using (var ws = new TestService())
                using (var wait = new ManualResetEvent(false))
                {
                    var proto = s.UseHttps ? "https" : "http";
                    ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                    var result = ws.SimpleMethod();
                    Assert.IsTrue(result.Contains("Hello World"));

                    ws.SimpleMethodCompleted += (sender, args) =>
                    {
                        Assert.IsNull(args.Error);
                        Assert.IsFalse(args.Cancelled);
                        Assert.IsNotNull(args.Result);
                        Assert.IsTrue(args.Result.Contains("Hello World"));

                        var w = (ManualResetEvent)args.UserState;
                        w.Set();
                    };

                    ws.SimpleMethodAsync(wait);

                    if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                    {
                        Assert.Fail("No response was received after 5 seconds.");
                    }
                }
            }
        }

        [TestMethod]
        public void TestHttpWeb()
        {
            var sm = ServerManager.Servers().StartAll();

            foreach (var s in sm.AllServers)
            {
                string protocol = s.UseHttps ? "https" : "http";

                var html = new WebClient().DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                Assert.IsTrue(html.Contains("Hello World"));
            }
        }

        [TestMethod]
        public void TestHttpAsmxWse()
        {
            var sm = ServerManager.Servers().StartAll();

            foreach (var s in sm.AllServers)
            {
                using (var ws = new TestServiceWse())
                using (var wait = new ManualResetEvent(false))
                {
                
                    var proto = s.UseHttps ? "https" : "http";
                    ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                    var result = ws.SimpleMethod();
                    Assert.IsTrue(result.Contains("Hello World"));

                    ws.SimpleMethodCompleted += (sender, args) =>
                    {
                        Assert.IsNull(args.Error);
                        Assert.IsFalse(args.Cancelled);
                        Assert.IsNotNull(args.Result);
                        Assert.IsTrue(args.Result.Contains("Hello World"));

                        var w = (ManualResetEvent)args.UserState;
                        w.Set();
                    };

                    ws.SimpleMethodAsync(wait);

                    if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                    {
                        Assert.Fail("No response was received after 5 seconds.");
                    }
                }
            }
        }

         [TestMethod]
        public void TestHttpWebHighVolume()
        {
            var sm = ServerManager.Servers().StartAll();

             foreach (var s in sm.AllServers)
             {
                 for (int i = 0; i < 25; i++)
                 {
                     string protocol = s.UseHttps ? "https" : "http";

                     var html = new WebClient().DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                     Debug.WriteLine(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                     Assert.IsTrue(html.Contains("Hello World"));
                 }
             }
        }

         [TestMethod]
         public void TestHttpAsmxHighVolume()
         {
             var sm = ServerManager.Servers().StartAll();

             foreach (var s in sm.AllServers)
             {
                 for (int i = 0; i < 25; i++)
                 {
                     using (var ws = new TestService())
                     using (var wait = new ManualResetEvent(false))
                     {
                         var proto = s.UseHttps ? "https" : "http";
                         ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                         var result = ws.SimpleMethod();
                         Assert.IsTrue(result.Contains("Hello World"));

                         ws.SimpleMethodCompleted += (sender, args) =>
                         {
                             Assert.IsNull(args.Error);
                             Assert.IsFalse(args.Cancelled);
                             Assert.IsNotNull(args.Result);
                             Assert.IsTrue(args.Result.Contains("Hello World"));

                             var w = (ManualResetEvent) args.UserState;
                             w.Set();
                         };

                         ws.SimpleMethodAsync(wait);

                         if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                         {
                             Assert.Fail("No response was received after 5 seconds.");
                         }
                     }
                 }
             }
         }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.34209")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "TestServiceSoap", Namespace = "http://localhost/")]
    public partial class TestServiceWse : Microsoft.Web.Services3.WebServicesClientProtocol
    {

        private System.Threading.SendOrPostCallback SimpleMethodOperationCompleted;

        private bool useDefaultCredentialsSetExplicitly;

        /// <remarks/>
        public TestServiceWse()
        {
            this.Url = global::Middleman.Tests.Properties.Settings.Default.Middleman_Tests_test_asmx_TestService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true))
            {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true)
                            && (this.useDefaultCredentialsSetExplicitly == false))
                            && (this.IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }

        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        /// <remarks/>
        public event SimpleMethodCompletedEventHandler SimpleMethodCompleted;

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://localhost/SimpleMethod", RequestNamespace = "http://localhost/", ResponseNamespace = "http://localhost/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string SimpleMethod()
        {
            object[] results = this.Invoke("SimpleMethod", new object[0]);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void SimpleMethodAsync()
        {
            this.SimpleMethodAsync(null);
        }

        /// <remarks/>
        public void SimpleMethodAsync(object userState)
        {
            if ((this.SimpleMethodOperationCompleted == null))
            {
                this.SimpleMethodOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSimpleMethodOperationCompleted);
            }
            this.InvokeAsync("SimpleMethod", new object[0], this.SimpleMethodOperationCompleted, userState);
        }

        private void OnSimpleMethodOperationCompleted(object arg)
        {
            if ((this.SimpleMethodCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SimpleMethodCompleted(this, new SimpleMethodCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }

        private bool IsLocalFileSystemWebService(string url)
        {
            if (((url == null)
                        || (url == string.Empty)))
            {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024)
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }
    }
}