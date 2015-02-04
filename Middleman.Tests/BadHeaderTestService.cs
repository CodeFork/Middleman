using System;
using System.Net;
using System.Reflection;
using Middleman.Tests.test.asmx;

namespace Middleman.Tests
{
    public class BadHeaderTestService : TestService
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var wr = base.GetWebRequest(uri) as HttpWebRequest;
            //wr.ProtocolVersion = HttpVersion.Version10;
            //wr.UserAgent = "Java1.4.2_10";
            ////wr.Connection = "close";
            //wr.Headers["Accept-Encoding"] = "application/soap+xml, application/xml, text/xml";
            //wr.KeepAlive = false;
            //wr.ContentType = "application/soap+xml";
            //wr.Accept = "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2";
            
            //wr.ServicePoint.Expect100Continue = false;

            //var sp = wr.ServicePoint;
            //var prop = sp.GetType().GetProperty("HttpBehaviour",
            //                        BindingFlags.Instance | BindingFlags.NonPublic);
            //prop.SetValue(sp, (byte)0, null);
            
            return wr;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var wr = request as HttpWebRequest;
            wr.ProtocolVersion = HttpVersion.Version10;
            wr.UserAgent = "Java1.4.2_10";
            //wr.Connection = "close";
            wr.Headers["Accept-Encoding"] = "application/soap+xml, application/xml, text/xml";
            wr.KeepAlive = false;
            //wr.ContentType = "application/soap+xml";
            wr.Accept = "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2";

            wr.ServicePoint.Expect100Continue = false;

            return base.GetWebResponse(request);
        }

    }
}