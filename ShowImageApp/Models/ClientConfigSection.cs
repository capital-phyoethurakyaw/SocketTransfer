using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShowImageApp.Models
{
    public class ClientConfigSection : ConfigurationSection
    {
        [System.Configuration.ConfigurationProperty("ClientList")]
        [ConfigurationCollection(typeof(ClientList), AddItemName = "Client")]
        public ClientList Clients
        {
            get
            {
                object o = this["ClientList"];
                return o as ClientList;
            }
        }
    }

    [ConfigurationCollection(typeof(ClientList))]
    public class ClientList : ConfigurationElementCollection
    {
        public Client this[int index]
        {
            get
            {
                return base.BaseGet(index) as Client;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new Client this[string responseString]
        {
            get { return (Client)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new Client();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((Client)element).IpAddress;
        }
    }

    public class Client : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("ipaddress", IsRequired = true)]
        public string IpAddress
        {
            get
            {
                return this["ipaddress"] as string;
            }
        }      
    }
}
