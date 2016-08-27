#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace AppsTracker.Data.Models
{
    public sealed class AppDuration
    {
        public string Name { get; set; }
        public double Duration { get; set; }
    }

    public sealed class UserLoggedTime
    {
        public string Username { get; set; }
        public double LoggedInTime { get; set; }
    }

    public sealed class ScreenshotModel
    {
        public string AppName { get; set; }
        public int Count { get; set; }
    }

    public sealed class UsageModel
    {
        public string Date { get; set; }
        public double Count { get; set; }
    }

    public sealed class AppDurationOverview
    {
        public string Date { get; set; }
        public List<AppDuration> AppCollection { get; set; }
    }

    public sealed class UsageSummary
    {
        public string UsageType { get; set; }
        public double Time { get; set; }
    }

    public sealed class UsageOverview
    {
        public string Date { get; set; }
        public DateTime DateInstance { get; set; }
        public ObservableCollection<UsageSummary> UsageCollection { get; set; }
    }

    public sealed class UsageByTime
    {
        public string Time { get; set; }
        public ObservableCollection<UsageSummary> UsageSummaryCollection { get; set; }
    }

    public sealed class DailyAppDuration
    {
        public string Date { get; set; }
        public double Duration { get; set; }
    }

    public sealed class DailyScreenshotModel
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public sealed class WindowDuration
    {
        public double Duration { get; set; }
        public string Title { get; set; }
    }

    public sealed class WindowDurationOverview
    {
        public string Date { get; set; }
        public List<WindowDuration> DurationCollection { get; set; }
    }

    public sealed class CategoryDuration
    {
        public string Name { get; set; }
        public double TotalTime { get; set; }
    }

    public sealed class DailyCategoryDuration
    {
        public string Date { get; set; }
        public double TotalTime { get; set; }
    }
}
