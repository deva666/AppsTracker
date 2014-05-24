using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Task_Logger_Pro.Controls
{
    class SelectableListView : ListView
    {


        public ObservableCollection<ISelectable> SelectableCollection
        {
            get { return (ObservableCollection<ISelectable>)GetValue(SelectableCollectionProperty); }
            set { SetValue(SelectableCollectionProperty, value); }
        }

        public static readonly DependencyProperty SelectableCollectionProperty =
            DependencyProperty.Register("SelectableCollection", typeof(ObservableCollection<ISelectable>), typeof(SelectableListView), new PropertyMetadata(null));

    }
}

