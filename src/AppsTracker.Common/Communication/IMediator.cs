#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;

namespace AppsTracker.Common.Communication
{
    public interface IMediator
    {
        void Register<T>(string message, Action<T> callBack);
        void Register(string message, Action callBack);
        void NotifyColleagues<T>(string message, T parameter);
        void NotifyColleagues(string message);
    }
}
