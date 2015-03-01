using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace AppsTracker.Widgets.Behavior
{
    internal sealed class FrameworkElementDropBehavior : Behavior<FrameworkElement>
    {
        private Type dataType; //the type of the data that can be dropped into this control
        private FrameworkElementAdorner adorner;

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
            if (dataType != null)
            {
                //if the data type can be dropped 
                if (e.Data.GetDataPresent(dataType))
                {
                    //drop the data
                    IDropable target = this.AssociatedObject.DataContext as IDropable;
                    target.Drop(e.Data.GetData(dataType));

                    //remove the data from the source
                    IDragable source = e.Data.GetData(dataType) as IDragable;
                    source.Remove(e.Data.GetData(dataType));
                }
            }
            if (this.adorner != null)
                this.adorner.Remove();

            e.Handled = true;
            return;
        }

        void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            if (this.adorner != null)
                this.adorner.Remove();
            e.Handled = true;
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (dataType != null)
            {
                //if item can be dropped
                if (e.Data.GetDataPresent(dataType))
                {
                    //give mouse effect
                    this.SetDragDropEffects(e);
                    //draw the dots
                    if (this.adorner != null)
                        this.adorner.Update();
                }
            }
            e.Handled = true;
        }

        void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            //if the DataContext implements IDropable, record the data type that can be dropped
            if (this.dataType == null)
            {
                if (this.AssociatedObject.DataContext != null)
                {
                    IDropable dropObject = this.AssociatedObject.DataContext as IDropable;
                    if (dropObject != null)
                    {
                        this.dataType = dropObject.DataType;
                    }
                }
            }

            if (this.adorner == null)
                this.adorner = new FrameworkElementAdorner(sender as UIElement);
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
