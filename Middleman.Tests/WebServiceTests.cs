using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using IISExpressAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.Services3;
using Middleman.Server.Server;
using Middleman.Tests.Properties;
using Middleman.Tests.test.asmx;

namespace Middleman.Tests
{
    [TestClass]
    public class WebServiceTests
    {
        private static IISExpress _iisExpress;

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; };

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
        public void TestHttpAsmxWithBadHeader()
        {
            var sm = ServerManager.Servers().StartAll();

            foreach (var s in sm.AllServers)
            {
                using (var ws = new BadHeaderTestService())
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

        [TestMethod]
        public void TestHttpWeb()
        {
            var sm = ServerManager.Servers().StartAll();

            foreach (var s in sm.AllServers)
            {
                var protocol = s.UseHttps ? "https" : "http";

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

        [TestMethod]
        public void TestHttpWebHighVolume()
        {
            var sm = ServerManager.Servers().StartAll();

            foreach (var s in sm.AllServers)
            {
                for (var i = 0; i < 25; i++)
                {
                    var protocol = s.UseHttps ? "https" : "http";

                    var html = new WebClient().DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                    Debug.WriteLine("{1}://localhost:{0}/", s.Port, protocol);

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
                for (var i = 0; i < 25; i++)
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

    /// <remarks />
    [GeneratedCode("System.Web.Services", "4.0.30319.34209")]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [WebServiceBinding(Name = "TestServiceSoap", Namespace = "http://localhost/")]
    public class TestServiceWse : WebServicesClientProtocol
    {
        private SendOrPostCallback SimpleMethodOperationCompleted;
        private bool useDefaultCredentialsSetExplicitly;

        /// <remarks />
        public TestServiceWse()
        {
            Url = Settings.Default.Middleman_Tests_test_asmx_TestService;
            if (IsLocalFileSystemWebService(Url))
            {
                UseDefaultCredentials = true;
                useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                useDefaultCredentialsSetExplicitly = true;
            }
        }

        public new string Url
        {
            get { return base.Url; }
            set
            {
                if (((IsLocalFileSystemWebService(base.Url)
                      && (useDefaultCredentialsSetExplicitly == false))
                     && (IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }

        public new bool UseDefaultCredentials
        {
            get { return base.UseDefaultCredentials; }
            set
            {
                base.UseDefaultCredentials = value;
                useDefaultCredentialsSetExplicitly = true;
            }
        }

        /// <remarks />
        public event SimpleMethodCompletedEventHandler SimpleMethodCompleted;

        /// <remarks />
        [SoapDocumentMethod("http://localhost/SimpleMethod", RequestNamespace = "http://localhost/", ResponseNamespace = "http://localhost/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public string SimpleMethod()
        {
            var results = Invoke("SimpleMethod", new object[0]);
            return ((string) (results[0]));
        }

        /// <remarks />
        public void SimpleMethodAsync()
        {
            SimpleMethodAsync(null);
        }

        /// <remarks />
        public void SimpleMethodAsync(object userState)
        {
            if ((SimpleMethodOperationCompleted == null))
            {
                SimpleMethodOperationCompleted = OnSimpleMethodOperationCompleted;
            }
            InvokeAsync("SimpleMethod", new object[0], SimpleMethodOperationCompleted, userState);
        }

        private void OnSimpleMethodOperationCompleted(object arg)
        {
            if ((SimpleMethodCompleted != null))
            {
                var invokeArgs = ((InvokeCompletedEventArgs) (arg));
                SimpleMethodCompleted(this, new SimpleMethodCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks />
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
            var wsUri = new Uri(url);
            if (((wsUri.Port >= 1024)
                 && (string.Compare(wsUri.Host, "localHost", StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }
    }

    public class BadHeaderTestService : TestService
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var wr = base.GetWebRequest(uri);
            wr.Headers["Accept-Encoding"] = "gzip, application/soap+xml, application/xml, text/xml";
            return wr;
        }
    }
}