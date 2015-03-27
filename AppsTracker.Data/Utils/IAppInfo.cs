#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion


namespace AppsTracker.Data.Utils
{
    public interface IAppInfo
    {
        string Name { get; }
        string Version { get; }
        string Company { get; }
        string Description { get; }
        string FileName { get; }
        string FullName { get; }
    }
}
