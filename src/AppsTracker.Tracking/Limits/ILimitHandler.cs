﻿using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Limits
{
    public interface ILimitHandler
    {
        void Handle(AppLimit warning);
    }
}
