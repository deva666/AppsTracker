using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppsTracker.Controls
{
    public partial class closeBtn : UserControl
    {
        public closeBtn()
        {
            InitializeComponent();
        }

        private void path_MouseEnter( object sender, System.Windows.Input.MouseEventArgs e )
        {
            VisualStateManager.GoToState( this, "MouseEnter", true );
        }

        private void path_MouseLeave( object sender, System.Windows.Input.MouseEventArgs e )
        {
            VisualStateManager.GoToState( this, "MouseLeave", true );
        }

        private void Path_MouseEnter_1( object sender, System.Windows.Input.MouseEventArgs e )
        {
            VisualStateManager.GoToState( this, "MouseEnter", true );
        }
    }
}
