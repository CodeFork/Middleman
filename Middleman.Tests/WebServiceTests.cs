using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using IISExpressAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Middleman.Server.Server;
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
        public void TestSingleHttpAsmx()
        {
            File.Delete(@"C:\Users\paul.mcilreavy\Dropbox\Development\Code\Middleman\Middleman.Tests\bin\Debug\Middleman.2015-02-04.log");
            var s = ServerManager.Servers().StartAll().AllServers[0];

            using (var ws = new BadHeaderTestService())
            using (var wait = new ManualResetEvent(false))
            {
                var proto = s.UseHttps ? "https" : "http";
                ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";
                var result = ws.SimpleMethod();
                Assert.IsTrue(result.Contains("Hello World"));
            }
        }


        [TestMethod]
        public void TestSingleHttpWeb()
        {
            var sm = ServerManager.Servers().StartAll();
            var s = sm.AllServers[0];

            var protocol = s.UseHttps ? "https" : "http";

            var html = new WebClient().DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

            Assert.IsTrue(html.Contains("Hello World"));
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
        }
    }
}