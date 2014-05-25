using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.ChartModels
{
    public abstract class Selectable
    {
        protected bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                SelectedChanging();
            }
        }

        protected void SelectedChanging()
        {
            var handler = IsSelectedChanging;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        public event EventHandler IsSelectedChanging;
    }
    public abstract class TopModel : Selectable
    {
        public double Usage { get; set; }
        public long Duration { get; set; }
        //public bool IsSelected { get; set; }
        public bool IsRequested { get; set; }
    }

    public class TopAppsModel : TopModel
    {
        public string AppName { get; set; }
        public string Date { get; set; }
        public DateTime DateTime { get; set; }

    }

    public class TopWindowsModel : TopModel
    {
        public string Title { get; set; }

    }

    public class DayViewModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateEnded { get; set; }
        public long Duration { get; set; }
        public bool IsRequested { get; set; }
        public bool IsSelected { get; set; }

    }
}
