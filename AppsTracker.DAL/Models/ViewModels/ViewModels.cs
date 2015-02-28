#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace AppsTracker.Data.Models
{
    public class MostUsedAppModel
    {
        string _appName;
        double _duration;

        public string AppName { get { return _appName; } set { _appName = value; } }
        public double Duration { get { return _duration; } set { _duration = value; } }
    }

    public class AllUsersModel
    {
        string _username;
        double _loggedInTime;

        public string Username { get { return _username; } set { _username = value; } }
        public double LoggedInTime { get { return _loggedInTime; } set { _loggedInTime = value; } }
    }

    public class BlockedAppModel
    {
        string _appName;
        int _count;
        public string AppName { get { return _appName; } set { _appName = value; } }
        public int Count { get { return _count; } set { _count = value; } }
    }

    public class KeystrokeModel
    {
        string _appName;
        int _count;

        public string AppName { get { return _appName; } set { _appName = value; } }
        public int Count { get { return _count; } set { _count = value; } }
    }

    public class ScreenshotModel
    {
        string _appName;
        int _count;

        public string AppName { get { return _appName; } set { _appName = value; } }
        public int Count { get { return _count; } set { _count = value; } }
    }

    public class UsageModel
    {
        string _date;
        double _count;

        public string Date { get { return _date; } set { _date = value; } }
        public double Count { get { return _count; } set { _count = value; } }
    }

    public class DailyUsedAppsSeries
    {
        public string Date { get; set; }
        public List<MostUsedAppModel> DailyUsedAppsCollection { get; set; }
    }

    public class UsageTypeModel
    {
        public string UsageType { get; set; }
        public double Time { get; set; }
    }

    public class UsageTypeSeries
    {
        public string Date { get; set; }
        public DateTime DateInstance { get; set; }
        public ObservableCollection<UsageTypeModel> DailyUsageTypeCollection { get; set; }
    }

    public class DailyUsageTypeSeries
    {
        public string Time { get; set; }
        public ObservableCollection<UsageTypeModel> DailyUsageTypeCollection { get; set; }
    }

    public class DailyTopWindowSeries
    {
        public string Date { get; set; }
        public ObservableCollection<TopWindowsModel> DailyUsageTypeCollection { get; set; }
    }

    public class DailyBlockedAppModel
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class DailyAppModel
    {
        string _date;
        double _duration;

        public string Date { get { return _date; } set { _date = value; } }
        public double Duration { get { return _duration; } set { _duration = value; } }
    }

    public class DailyKeystrokeModel
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class DailyScreenshotModel
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class FilewatcherModel
    {
        public string Event { get; set; }
        public int Count { get; set; }
    }

    public class DailyWindowDurationModel
    {
        public double Duration { get; set; }
        public string Title { get; set; }
    }

    public class DailyWindowSeries
    {
        public string Date { get; set; }
        public List<DailyWindowDurationModel> DailyWindowCollection { get; set; }
    }

}
