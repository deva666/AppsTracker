using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Utils
{
   [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
   sealed class SettingsAttribute : Attribute
   {

      public SettingsAttribute(string propertyName, object defaultValue = null)
      {
         PropertyName = propertyName;
         DefaultValue = defaultValue;
      }

      public string PropertyName { get; private set; }

      public Object DefaultValue { get; private set; }


   }
}
