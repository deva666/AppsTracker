using System.Windows.Controls;
using Task_Logger_Pro.Pages.ViewModels;

namespace Task_Logger_Pro.Pages
{
    /// <summary>
    /// Interaction logic for Data_computerUsage.xaml
    /// </summary>
    public partial class Data_computerUsage : Page
    {
        public Data_computerUsage()
        {
            InitializeComponent();
            this.DataContext = new Data_computerUsageViewModel();
        }
    }
}
