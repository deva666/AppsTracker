using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppsTracker
{
    public class WaitCursor : IDisposable
    {
   
        public WaitCursor()
        {
            //_previousCursor = Mouse.OverrideCursor;

            //Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }

}
