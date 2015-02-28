#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion


namespace AppsTracker.Data.Utils
{
    public interface IAppInfo
    {
        string ProcessName { get; }
        string ProcessVersion { get; }
        string ProcessCompany { get; }
        string ProcessDescription { get; }
        string ProcessFileName { get; }
        string ProcessRealName { get; }
    }
}
