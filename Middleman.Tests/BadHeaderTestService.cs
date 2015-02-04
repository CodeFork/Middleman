using System;
using System.Net;
using System.Reflection;
using Middleman.Tests.test.asmx;

namespace Middleman.Tests
{
    public class BadHeaderTestService : TestService
    {
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var wr = request as HttpWebRequest;
            //wr.ProtocolVersion = HttpVersion.Version10;
            //wr.UserAgent = "Java1.4.2_10";
            wr.Headers["Accept-Encoding"] = "application/soap+xml, application/xml, text/xml";
            //wr.KeepAlive = false;
            //wr.Accept = "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2";

            //wr.ServicePoint.Expect100Continue = false;

            return base.GetWebResponse(request);
        }
    }

    public class Http10TestService : TestService
    {
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var wr = request as HttpWebRequest;

            wr.ProtocolVersion = HttpVersion.Version10;

            return base.GetWebResponse(request);
        }
    }


    public class Http10NoKeepAliveTestService : TestService
    {
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var wr = request as HttpWebRequest;

            wr.ProtocolVersion = HttpVersion.Version10;
            wr.KeepAlive = false;

            return base.GetWebResponse(request);
        }
    }
}