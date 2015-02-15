using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.XmlSettings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class SettingsNodeAttribute : Attribute
    {
        private object defaultValue;

        public SettingsNodeAttribute(object nodeDefaultValue = null)
        {
            defaultValue = nodeDefaultValue;
        }

        public object DefaultValue { get { return defaultValue; } }
    }
}
