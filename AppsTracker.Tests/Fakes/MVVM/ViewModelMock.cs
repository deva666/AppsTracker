#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.ServiceLocation;

namespace AppsTracker.Tests.Fakes.MVVM
{
    public class ViewModelMock : ViewModelBase
    {
        public override string Title
        {
            get { return "Mock"; }
        }
    }
}
