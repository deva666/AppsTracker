using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppsTracker.Utils
{
    internal abstract class SettingsBase
    {
        public abstract string RootNodeName
        {
            get;
            protected set;
        }

        public virtual XElement CreateXml()
        {
            var root = new XElement(RootNodeName);
            foreach (var prop in this.GetType().GetProperties())
            {
                var attr = prop.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault();
                if( attr == null)
                    continue;
                var subNode = new XElement(((SettingsAttribute)attr).PropertyName);
                
            }

            return root;
        }

        public virtual void FromXml(XElement node)
        {

        }
    }
}
