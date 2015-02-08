using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.Controllers
{    
    public interface IAppearanceController
    {
        void Initialize(Setting settings);
        void SettingsChanging(Setting settings);
    }
}
