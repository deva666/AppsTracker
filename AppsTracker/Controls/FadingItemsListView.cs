using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AppsTracker.Controls
{
    class FadingItemsListView : FilterableListView
    {
        public FadingItemsListView()
        {
            FadeInAllItems();
            this.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                FadeInAllItems();
        }

        private void FadeInAllItems()
        {
            ListViewItem[] items = new ListViewItem[this.Items.Count];
            for (int i = 0; i < this.Items.Count; i++)
            {
                items[i] = (ListViewItem)this.ItemContainerGenerator.ContainerFromIndex(i);
            }
            foreach (var item in items)
            {
                if(item != null)
                item.Opacity = 0;
            }
            var enumerator = items.GetEnumerator();
            if (enumerator.MoveNext())
            {
                DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
                timer.Tick += (s, e) =>
                {
                    var item = enumerator.Current;
                    if (item != null)
                    ((ListViewItem)item).Opacity = 1;
                    if (!enumerator.MoveNext())
                    {
                        timer.Stop();
                    }
                };
                timer.Start();
            }
            //foreach (var item in this.Items)
            //{
            //    ListViewItem lvi = (ListViewItem)this.ItemContainerGenerator.ContainerFromItem(item);
            //    if (lvi != null)
            //    {
            //        lvi.Opacity = 0;
            //        Items.Add(lvi);
            //    }
            //}
            //foreach (var item in _items)
            //{
            //    DispatcherTimer timer = new DispatcherTimer();
            //    timer.Interval = TimeSpan.FromMilliseconds(1000);
            //    timer.Tick += (s, e) => item.Opacity = 1;
            //    timer.Start();

            //}

        }

    }
}
