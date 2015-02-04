using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using Microsoft.Web.Services3;
using Middleman.Tests.Properties;
using Middleman.Tests.test.asmx;

namespace Middleman.Tests
{
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
}