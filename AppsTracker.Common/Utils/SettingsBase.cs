using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Common.Utils
{
    [Serializable]
    public class SettingsBase
    {

        [OnDeserializing]
        private void SetDefaults(StreamingContext context)
        {

        }
    }
}
