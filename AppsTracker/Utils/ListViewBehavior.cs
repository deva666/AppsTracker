using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;
using AppsTracker.Models.EntityModels;
using Task_Logger_Pro.Controls;

namespace Task_Logger_Pro.Utils
{
    public class ListViewBehavior : Behavior<FilterableListView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        void AssociatedObject_SelectionChanged(object sender,
                                               SelectionChangedEventArgs e)
        {
            if (sender is FilterableListView)
            {
                FilterableListView listview = (sender as FilterableListView);
                List<Log> loglist = listview.Items.SourceCollection as List<Log>;
            //    listview.ScrollIntoView(loglist.First(l => l.IsSelected));
                //if (listview.SelectedItem != null)
                //{
                //    listview.Dispatcher.BeginInvoke(
                //        (Action)(() =>
                //        {
                //            listview.UpdateLayout();
                //            if (listview.SelectedItem !=
                //                null)
                //                listview.ScrollIntoView(
                //                    listview.SelectedItem);
                //        }));
                //}
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;

        }
    }
}
