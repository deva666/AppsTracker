using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Utils
{
   internal sealed class SettingsRootNodeName : Attribute
   {
      public string Name { get; private set; }
      public SettingsRootNodeName(string name)
      {
         Name = name;
      }
   }
}
