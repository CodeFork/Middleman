using System;
using System.Configuration;

namespace Middleman.Server.Configuration
{
    public class ListenerConfiguration : ConfigurationElement
    {
        public ListenerConfiguration() { }

        public ListenerConfiguration(int listenPort, bool listenSsl, string sslCertName, string destinationHost)
        {
            ListenPort = listenPort;
            DestinationHost = destinationHost;
            ListenSsl = listenSsl;
            SslCertName = sslCertName;
        }

        [ConfigurationProperty("ListenSsl", IsRequired = false, DefaultValue = false, IsKey = false)]
        public bool ListenSsl
        {
            get { return Convert.ToBoolean(this["ListenSsl"]); }
            set { this["ListenSsl"] = value; }
        }

        [ConfigurationProperty("ListenPort", IsRequired = true, IsKey = true)]
        public int ListenPort
        {
            get { return (int)this["ListenPort"]; }
            set { this["ListenPort"] = value; }
        }

        [ConfigurationProperty("DestinationHost", IsRequired = true, IsKey = false)]
        public string DestinationHost
        {
            get { return (string)this["DestinationHost"]; }
            set { this["DestinationHost"] = value; }
        }

        [ConfigurationProperty("SslCertName", IsRequired = false, DefaultValue = null, IsKey = false)]
        public string SslCertName
        {
            get { return (string)this["SslCertName"]; }
            set { this["SslCertName"] = value; }
        }
    }
}
