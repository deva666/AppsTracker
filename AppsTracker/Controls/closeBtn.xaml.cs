#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Windows;
using System.Windows.Controls;

namespace AppsTracker.Controls
{
    public partial class closeBtn : UserControl
    {
        public closeBtn()
        {
            InitializeComponent();
        }

        private void path_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseEnter", true);
        }

        private void path_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseLeave", true);
        }

        private void Path_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseEnter", true);
        }
    }
}
