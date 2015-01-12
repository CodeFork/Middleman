using System.Configuration;

namespace Middleman.Server.Configuration
{
    public class ListenerConfigurationCollection : ConfigurationElementCollection
    {
        public ListenerConfigurationCollection()
        {

        }

        public ListenerConfiguration this[int index]
        {
            get { 
                return (ListenerConfiguration)BaseGet(index); 
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(ListenerConfiguration serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ListenerConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ListenerConfiguration)element).ListenPort;
        }

        public void Remove(ListenerConfiguration serviceConfig)
        {
            BaseRemove(serviceConfig.ListenPort);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}