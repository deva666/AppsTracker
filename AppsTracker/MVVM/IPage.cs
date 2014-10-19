using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppsTracker.MVVM
{
    interface IPage
    {
        int Pages { get; set; }
        int CurrentPage { get; set; }
        int ItemsCount { get; set; }
        IEnumerable<int> AllPages { get; }
        ICommand LoadNextPageCommand { get; }
        ICommand LoadPreviousPageCommand { get; }
        
    }
}
