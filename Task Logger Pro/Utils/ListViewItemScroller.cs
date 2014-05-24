using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Task_Logger_Pro.Utils
{
    public static class ListViewItemScroller
    {

        #region IsBroughtIntoViewWhenSelected

        /// <summary>
        /// Gets the IsBroughtIntoViewWhenSelected value
        /// </summary>
        /// <param name="listViewItem"></param>
        /// <returns></returns>
        public static bool GetIsBroughtIntoViewWhenSelected(ListViewItem listViewItem)
        {
            return (bool)listViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
        }

        /// <summary>
        /// Sets the IsBroughtIntoViewWhenSelected value
        /// </summary>
        /// <param name="listViewItem"></param>
        /// <param name="value"></param>
        public static void SetIsBroughtIntoViewWhenSelected(
          ListViewItem listViewItem, bool value)
        {
            listViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        /// <summary>
        /// Determins if the ListBoxItem is bought into view when enabled
        /// </summary>
        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(ListViewItemScroller),
            new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        /// <summary>
        /// Action to take when item is brought into view
        /// </summary>
        /// <param name="depObj"></param>
        /// <param name="e"></param>
        static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            ListViewItem item = depObj as ListViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;
            
            if ((bool)e.NewValue)
                item.Selected += OnListBoxItemSelected;
            else
                item.Selected -= OnListBoxItemSelected;
        }

        static void OnListBoxItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the ListBoxItem 
            // whose IsSelected property was modified.  Ignore all ancestors 
            // who are merely reporting that a descendant's Selected fired. 
            if (!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            ListViewItem item = e.OriginalSource as ListViewItem;
            if (item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}

