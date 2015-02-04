using System;
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
        private static ServerManager _serviceManager;

        [TestInitialize]
        public void TestInit()
        {
            string logDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            string logFile = string.Format("Middleman.{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            string logFilepath = Path.Combine(logDir, logFile);
            if (File.Exists(logFilepath))
            {
                File.Delete(logFilepath);
            }
        }

        [ClassInitialize]
        public static void Init(TestContext ctx)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; };

            _serviceManager = ServerManager.Servers().StartAll();

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

            if (_serviceManager != null)
            {
                _serviceManager.Stop();
                _serviceManager = null;
            }
        }

        [TestMethod]
        public void TestHttpAsmxWithBadHeader()
        {
            foreach (var s in _serviceManager.AllServers)
            {
                Console.WriteLine(s.Port);
                using (var ws = new BadHeaderTestService())
                //using (var wait = new ManualResetEvent(false))
                {
                    var proto = s.UseHttps ? "https" : "http";
                    ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                    var result = ws.SimpleMethod();
                    Assert.IsTrue(result.Contains("Hello World"));

                    //ws.SimpleMethodCompleted += (sender, args) =>
                    //{
                    //    Assert.IsNull(args.Error);
                    //    Assert.IsFalse(args.Cancelled);
                    //    Assert.IsNotNull(args.Result);
                    //    Assert.IsTrue(args.Result.Contains("Hello World"));

                    //    var w = (ManualResetEvent)args.UserState;
                    //    w.Set();
                    //};

                    //ws.SimpleMethodAsync(wait);

                    //if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                    //{
                    //    Assert.Fail("No response was received after 5 seconds.");
                    //}
                    ws.Dispose();
                }
            }
        }

        [TestMethod]
        public void TestHttpAsmx()
        {
            foreach (var s in _serviceManager.AllServers)
            {
                Debug.WriteLine(s.Port);
                using (var ws = new TestService())
                //using (var wait = new ManualResetEvent(false))
                {
                    var proto = s.UseHttps ? "https" : "http";
                    ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                    var result = ws.SimpleMethod();
                    Assert.IsTrue(result.Contains("Hello World"));

                    //ws.SimpleMethodCompleted += (sender, args) =>
                    //{
                    //    Assert.IsNull(args.Error);
                    //    Assert.IsFalse(args.Cancelled);
                    //    Assert.IsNotNull(args.Result);
                    //    Assert.IsTrue(args.Result.Contains("Hello World"));

                    //    var w = (ManualResetEvent)args.UserState;
                    //    w.Set();
                    //};

                    //ws.SimpleMethodAsync(wait);

                    //if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                    //{
                    //    Assert.Fail("No response was received after 5 seconds.");
                    //}

                    ws.Dispose();
                }
            }
        }

        [TestMethod]
        public void TestSingleHttpAsmx()
        {
            var s = _serviceManager.AllServers[0];

            using (var ws = new TestService())
            {
                var proto = s.UseHttps ? "https" : "http";
                ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";
                //ws.UserAgent = "Java1.4.2_10";

                var result = ws.SimpleMethod();
                Assert.IsTrue(result.Contains("Hello World"));

                ws.Dispose();
            }
        }

        [TestMethod]
        public void TestSingleHttpAsmxWithBadHeader()
        {
            var s = _serviceManager.AllServers[0];

            using (var ws = new BadHeaderTestService())
            {
                var proto = s.UseHttps ? "https" : "http";
                ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";
                //ws.UserAgent = "Java1.4.2_10";

                var result = ws.SimpleMethod();
                Assert.IsTrue(result.Contains("Hello World"));

                ws.Dispose();
            }
        }

        [TestMethod]
        public void TestSingleHttpAsmxWithHttp10()
        {
            var s = _serviceManager.AllServers[0];

            using (var ws = new Http10TestService())
            {
                var proto = s.UseHttps ? "https" : "http";
                ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";
                //ws.UserAgent = "Java1.4.2_10";

                var result = ws.SimpleMethod();
                Assert.IsTrue(result.Contains("Hello World"));

                ws.Dispose();
            }
        }


        [TestMethod]
        public void TestSingleHttpAsmxWithHttp10NoKeepAlive()
        {
            var s = _serviceManager.AllServers[0];

            using (var ws = new Http10NoKeepAliveTestService())
            {
                var proto = s.UseHttps ? "https" : "http";
                ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";
                //ws.UserAgent = "Java1.4.2_10";

                var result = ws.SimpleMethod();
                Assert.IsTrue(result.Contains("Hello World"));

                ws.Dispose();
            }
        }

        [TestMethod]
        public void TestSingleHttpWeb()
        {
            var s = _serviceManager.AllServers[0];

            var protocol = s.UseHttps ? "https" : "http";

            using (var wc = new WebClient())
            {
                var html = wc.DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                Assert.IsTrue(html.Contains("Hello World"));
            }
        }

        [TestMethod]
        public void TestHttpWeb()
        {
            foreach (var s in _serviceManager.AllServers)
            {
                Console.WriteLine(s.Port);
                var protocol = s.UseHttps ? "https" : "http";

                var html = new WebClient().DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                Assert.IsTrue(html.Contains("Hello World"));
            }
        }

        [TestMethod]
        public void TestHttpAsmxWse()
        {
            foreach (var s in _serviceManager.AllServers)
            {
                Console.WriteLine(s.Port);
                using (var ws = new TestServiceWse())
                //using (var wait = new ManualResetEvent(false))
                {
                    var proto = s.UseHttps ? "https" : "http";
                    ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                    var result = ws.SimpleMethod();
                    Assert.IsTrue(result.Contains("Hello World"));

                    //ws.SimpleMethodCompleted += (sender, args) =>
                    //{
                    //    Assert.IsNull(args.Error);
                    //    Assert.IsFalse(args.Cancelled);
                    //    Assert.IsNotNull(args.Result);
                    //    Assert.IsTrue(args.Result.Contains("Hello World"));

                    //    var w = (ManualResetEvent)args.UserState;
                    //    w.Set();
                    //};

                    //ws.SimpleMethodAsync(wait);

                    //if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                    //{
                    //    Assert.Fail("No response was received after 5 seconds.");
                    //}
                    ws.Dispose();
                }
            }
        }

        [TestMethod]
        public void TestHttpWebHighVolume()
        {
            using (var wc = new WebClient())
            {
                foreach (var s in _serviceManager.AllServers)
                {
                    Console.WriteLine(s.Port);
                    for (var i = 0; i < 25; i++)
                    {
                        var protocol = s.UseHttps ? "https" : "http";

                        var html = wc.DownloadString(string.Format("{1}://localhost:{0}/", s.Port, protocol));

                        Debug.WriteLine("{1}://localhost:{0}/", s.Port, protocol);

                        Assert.IsTrue(html.Contains("Hello World"));


                    }
                }
            }
        }

        [TestMethod]
        public void TestHttpAsmxHighVolume()
        {
            foreach (var s in _serviceManager.AllServers)
            {
                for (var i = 0; i < 25; i++)
                {
                    using (var ws = new TestService())
                    //using (var wait = new ManualResetEvent(false))
                    {
                        var proto = s.UseHttps ? "https" : "http";
                        ws.Url = proto + "://127.0.0.1:" + s.Port + "/TestService.asmx";

                        var result = ws.SimpleMethod();
                        Assert.IsTrue(result.Contains("Hello World"));

                        //ws.SimpleMethodCompleted += (sender, args) =>
                        //{
                        //    Assert.IsNull(args.Error);
                        //    Assert.IsFalse(args.Cancelled);
                        //    Assert.IsNotNull(args.Result);
                        //    Assert.IsTrue(args.Result.Contains("Hello World"));

                        //    var w = (ManualResetEvent)args.UserState;
                        //    w.Set();
                        //};

                        //ws.SimpleMethodAsync(wait);

                        //if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                        //{
                        //    Assert.Fail("No response was received after 5 seconds.");
                        //}
                        ws.Dispose();
                    }
                }
            }
        }
    }
}