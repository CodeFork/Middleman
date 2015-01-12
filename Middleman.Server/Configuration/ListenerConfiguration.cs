using System;
using System.Configuration;
using NLog;

namespace Middleman.Server.Configuration
{
    public class ListenerConfiguration : ConfigurationElement
    {
        public ListenerConfiguration()
        {

        }

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
            get
            {
                var v = Convert.ToBoolean(this["ListenSsl"]);
                return v; 
            }
            set 
            {
                this["ListenSsl"] = value; 
            }
        }

        [ConfigurationProperty("ListenPort", IsRequired = true, IsKey = true)]
        public int ListenPort
        {
            get 
            {
                var v = (int)this["ListenPort"];
                return v;
            }
            set 
            {
                this["ListenPort"] = value;
            }
        }

        [ConfigurationProperty("DestinationHost", IsRequired = true, IsKey = false)]
        public string DestinationHost
        {
            get 
            {
                var v = (string)this["DestinationHost"];
                return (string)this["DestinationHost"];
            }
            set 
            {
                this["DestinationHost"] = value; 
            }
        }

        [ConfigurationProperty("SslCertName", IsRequired = false, DefaultValue = null, IsKey = false)]
        public string SslCertName
        {
            get
            {
                var v = (string)this["SslCertName"];
                return (string)this["SslCertName"]; 
            }
            set
            {
                this["SslCertName"] = value;
            }
        }
    }
}
