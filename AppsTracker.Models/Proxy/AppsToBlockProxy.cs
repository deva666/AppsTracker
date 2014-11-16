#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Utils;

namespace AppsTracker.Models.Proxy
{
    public class AppsToBlockProxy : ObservableObject
    {
        public AppsToBlockProxy(AppsToBlock appToBlock)
        {
            AppsToBlockID = appToBlock.AppsToBlockID;
            UserID = appToBlock.UserID;
            ApplicationID = appToBlock.ApplicationID;
            this.Application = appToBlock.Application;

            _monday = appToBlock.Monday;
            _tuesday = appToBlock.Tuesday;
            _wednesday = appToBlock.Wednesday;
            _thursday = appToBlock.Thursday;
            _friday = appToBlock.Friday;
            _saturday = appToBlock.Saturday;
            _sunday = appToBlock.Sunday;

            _timeMax = appToBlock.TimeMax;
            _timeMin = appToBlock.TimeMin;
        }

        public int AppsToBlockID { get; private set; }

        public int UserID { get; set; }

        public int ApplicationID { get; set; }

        bool _monday;
        public bool Monday
        {
            get
            {
                return _monday;
            }
            set
            {
                _monday = value;
                PropertyChanging("Monday");
            }
        }

        bool _tuesday;
        public bool Tuesday
        {
            get
            {
                return _tuesday;
            }
            set
            {
                _tuesday = value;
                PropertyChanging("Tuesday");
            }
        }

        bool _wednesday;
        public bool Wednesday
        {
            get
            {
                return _wednesday;
            }
            set
            {
                _wednesday = value;
                PropertyChanging("Wednesday");
            }
        }

        bool _thursday;
        public bool Thursday
        {
            get
            {
                return _thursday;
            }
            set
            {
                _thursday = value;
                PropertyChanging("Thursday");
            }
        }

        bool _friday;
        public bool Friday
        {
            get
            {
                return _friday;
            }
            set
            {
                _friday = value;
                PropertyChanging("Friday");
            }
        }

        bool _saturday;
        public bool Saturday
        {
            get
            {
                return _saturday;
            }
            set
            {
                _saturday = value;
                PropertyChanging("Saturday");
            }
        }

        bool _sunday;
        public bool Sunday
        {
            get
            {
                return _sunday;
            }
            set
            {
                _sunday = value;
                PropertyChanging("Sunday");
            }
        }

        long _timeMin;
        public long TimeMin
        {
            get
            {
                return _timeMin;
            }
            set
            {
                _timeMin = value;
                PropertyChanging("TimeMin");
            }
        }

        long _timeMax;
        public long TimeMax
        {
            get
            {
                return _timeMax;
            }
            set
            {
                _timeMax = value;
                PropertyChanging("TimeMax");
            }
        }

        public Aplication Application { get; private set; }

        public static implicit operator AppsToBlockProxy(AppsToBlock appToBlock)
        {
            AppsToBlockProxy proxy = new AppsToBlockProxy(appToBlock);
            return proxy;
        }
    }
}
