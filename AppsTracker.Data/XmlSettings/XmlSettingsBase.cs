using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppsTracker.Data.XmlSettings
{
    public abstract class XmlSettingsBase
    {
        public virtual XElement GetXML()
        {
            var properties = GetSettingsProperties();

            var xml = new XElement(this.GetType().Name);

            foreach (var prop in properties)
            {
                var node = new XElement(prop.Name);
                node.Add(prop.GetValue(this, BindingFlags.Default, null, null, CultureInfo.InvariantCulture));
                xml.Add(node);
            }

            return xml;
        }

        public virtual void SetValues(XElement xml)
        {
            if (xml == null)
                throw new ArgumentNullException("xml");

            var properties = GetSettingsProperties();

            foreach (var prop in properties)
            {
                var node = xml.Element(prop.Name);
                if (node == null)
                {
                    var settingsNode = (SettingsNodeAttribute)prop.GetCustomAttributes(typeof(SettingsNodeAttribute), true)[0];

                    if (settingsNode.DefaultValue != null)
                        prop.SetValue(this, settingsNode.DefaultValue);

                    continue;
                }

                prop.SetValue(this, GetNodeValue(prop, node));
            }
        }

        private IEnumerable<PropertyInfo> GetSettingsProperties()
        {
            return this.GetType().GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(SettingsNodeAttribute)));
        }

        private object GetNodeValue(PropertyInfo property, XElement node)
        {
            var type = property.PropertyType;
            if (type == typeof(string))
                return node.Value;
            else if (type == typeof(bool))
                return bool.Parse(node.Value);
            else if (type == typeof(int))
                return int.Parse(node.Value);
            else if (type == typeof(double))
                return double.Parse(node.Value,
                    NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            else if (type == typeof(DateTime))
                return DateTime.Parse(node.Value, CultureInfo.InvariantCulture);
            else
                throw new InvalidOperationException(type.ToString()
                    + " is not supported in Settings class");
        }
    }
}
