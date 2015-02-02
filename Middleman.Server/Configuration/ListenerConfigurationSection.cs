using System.Configuration;

namespace Middleman.Server.Configuration
{
    public class ListenerConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("Listeners", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(ListenerConfigurationCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public ListenerConfigurationCollection Listeners
        {
            get { return (ListenerConfigurationCollection)base["Listeners"]; }
        }
    }
}