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
using System.Windows.Controls.Primitives;
using Task_Logger_Pro.Pages.ViewModels;

namespace Task_Logger_Pro.Pages
{
    /// <summary>
    /// Interaction logic for Keylogs.xaml
    /// </summary>
    public partial class Keylogs : Page
    {
        public Keylogs()
        {
            InitializeComponent();
            Console.WriteLine("Keylogs constructor called");
        }

    }
}
