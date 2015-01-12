using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using IISExpressAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Middleman.Server.Handlers;
using Middleman.Server.Server;

using Middleman.Tests.test.asmx;

namespace Middleman.Tests
{
    [TestClass]
    public class WebServiceTests
    {
        [TestMethod]
        public void TestHttpAsmx()
        {

            string projDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.FullName;
            string webroot = Path.Combine(projDir, "Websites");

            //using (var iis = new IISExpress(new Parameters
            //{
            //    Path = webroot,
            //    Port = 5566
            //}))
            //{


                var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6655);
                //var handler = new SimpleReverseProxyHandler("http://localhost:5566/");
                var handler = new SimpleReverseProxyHandler("http://localhost:56934/");
                var server = new MiddlemanServer(endPoint, handler);

                server.Start();

                using (var ws = new TestService())
                using (var wait = new ManualResetEvent(false))
                {
                    ws.Url = "http://127.0.0.1:6655/TestService.asmx";

                    var s = ws.SimpleMethod();

                    ws.SimpleMethodCompleted += (sender, args) =>
                    {
                        Assert.IsNull(args.Error);
                        Assert.IsFalse(args.Cancelled);
                        Assert.IsNotNull(args.Result);

                        var w = (ManualResetEvent) args.UserState;
                        w.Set();
                    };

                    ws.SimpleMethodAsync(wait);

                    if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
                    {
                        Assert.Fail("No response was received after 5 seconds.");
                    }
                }
            //}
        }

        //[TestMethod]
        //public void TestHttpWcf()
        //{
        //    var endPoint = new IPEndPoint(IPAddress.Loopback, 8080);
        //    var handler = new SimpleReverseProxyHandler("http://dev.virtualearth.net/");
        //    var server = new MiddlemanServer(endPoint, handler);

        //    server.Start();

        //    var binding = new BasicHttpBinding();
        //    using (var ws = new SearchServiceClient(binding, new EndpointAddress("http://127.0.0.1:8080/webservices/v1/searchservice/searchservice.svc")))
        //    using (var wait = new ManualResetEvent(false))
        //    {
        //        Task.WaitAll(new[] { ws.SearchAsync(new SearchRequest { Query = "Edinburgh" }) }, 5000);

        //        //ws.GetWeatherCompleted += (sender, args) =>
        //        //{
        //        //    Assert.IsNull(args.Error);
        //        //    Assert.IsFalse(args.Cancelled);
        //        //    Assert.IsNotNull(args.Result);

        //        //    var w = (ManualResetEvent)args.UserState;
        //        //    w.Set();
        //        //};

        //        //ws.GetWeatherAsync("Brisbane", "Australia", wait);

        //        //if (!wait.WaitOne(Debugger.IsAttached ? -1 : 5000))
        //        //{
        //        //    Assert.Fail("No response was received after 5 seconds.");
        //        //}
        //    }
        //}
    }
}