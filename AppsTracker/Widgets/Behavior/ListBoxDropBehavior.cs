using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace AppsTracker.Widgets.Behavior
{
    public class ListBoxDropBehavior : Behavior<ItemsControl>
    {
        private Type dataType; //the type of the data that can be dropped into this control
        private ListBoxAdornerManager insertAdornerManager;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.DragEnter += new DragEventHandler(AssociatedObject_DragEnter);
            this.AssociatedObject.DragOver += new DragEventHandler(AssociatedObject_DragOver);
            this.AssociatedObject.DragLeave += new DragEventHandler(AssociatedObject_DragLeave);
            this.AssociatedObject.Drop += new DragEventHandler(AssociatedObject_Drop);
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            //if the data type can be dropped 
            if (this.dataType != null)
            {
                if (e.Data.GetDataPresent(dataType))
                {
                    //first find the UIElement that it was dropped over, then we determine if it's 
                    //dropped above or under the UIElement, then insert at the correct index.
                    ItemsControl dropContainer = sender as ItemsControl;
                    //get the UIElement that was dropped over
                    UIElement droppedOverItem = UIHelper.GetUIElement(dropContainer, e.GetPosition(dropContainer));
                    int dropIndex = -1; //the location where the item will be dropped
                    dropIndex = dropContainer.ItemContainerGenerator.IndexFromContainer(droppedOverItem) + 1;
                    //find if it was dropped above or below the index item so that we can insert 
                    //the item in the correct place
                    if (UIHelper.IsPositionAboveElement(droppedOverItem, e.GetPosition(droppedOverItem))) //if above
                    {
                        dropIndex = dropIndex - 1; //we insert at the index above it
                    }
                    //remove the data from the source
                    IDragable source = e.Data.GetData(dataType) as IDragable;
                    source.Remove(e.Data.GetData(dataType));

                    //drop the data
                    IDropable target = this.AssociatedObject.DataContext as IDropable;
                    target.Drop(e.Data.GetData(dataType), dropIndex);
                }
            }
            if (this.insertAdornerManager != null)
                this.insertAdornerManager.Clear();
            e.Handled = true;
            return;
        }

        void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            if (this.insertAdornerManager != null)
                this.insertAdornerManager.Clear();
            e.Handled = true;
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (this.dataType != null)
            {
                if (e.Data.GetDataPresent(dataType))
                {
                    this.SetDragDropEffects(e);
                    if (this.insertAdornerManager != null)
                    {
                        ItemsControl dropContainer = sender as ItemsControl;
                        UIElement droppedOverItem = UIHelper.GetUIElement(dropContainer, e.GetPosition(dropContainer));
                        bool isAboveElement = UIHelper.IsPositionAboveElement(droppedOverItem, e.GetPosition(droppedOverItem));
                        this.insertAdornerManager.Update(droppedOverItem, isAboveElement);
                    }
                }
            }
            e.Handled = true;
        }

        void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            if (this.dataType == null)
            {
                //if the DataContext implements IDropable, record the data type that can be dropped
                if (this.AssociatedObject.DataContext != null)
                {
                    if (this.AssociatedObject.DataContext as IDropable != null)
                    {
                        this.dataType = ((IDropable)this.AssociatedObject.DataContext).DataType;
                    }
                }
            }
            //initialize adorner manager with the adorner layer of the itemsControl
            if (this.insertAdornerManager == null)
                this.insertAdornerManager = new ListBoxAdornerManager(AdornerLayer.GetAdornerLayer(sender as ItemsControl));

            e.Handled = true;
        }

        /// <summary>
        /// Provides feedback on if the data can be dropped
        /// </summary>
        /// <param name="e"></param>
        private void SetDragDropEffects(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;  //default to None

            //if the data type can be dropped 
            if (e.Data.GetDataPresent(dataType))
            {
                e.Effects = DragDropEffects.Move;
            }
        }
    }
}
