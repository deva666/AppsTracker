using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace AppsTracker.Widgets.Behavior
{
    internal class ListBoxAdornerManager
    {
        private AdornerLayer adornerLayer;
        private ListBoxAdorner adorner;

        private bool shouldCreateNewAdorner = false;

        internal ListBoxAdornerManager(AdornerLayer layer)
        {
            this.adornerLayer = layer;
        }

        internal void Update(UIElement adornedElement, bool isAboveElement)
        {
            if (adorner != null && !shouldCreateNewAdorner)
            {
                //exit if nothing changed
                if (adorner.AdornedElement == adornedElement && adorner.IsAboveElement == isAboveElement)
                    return;
            }
            this.Clear();
            //draw new adorner
            adorner = new ListBoxAdorner(adornedElement, this.adornerLayer);
            adorner.IsAboveElement = isAboveElement;
            adorner.Update();
            this.shouldCreateNewAdorner = false;
        }


        /// <summary>
        /// Remove the adorner 
        /// </summary>
        internal void Clear()
        {
            if (this.adorner != null)
            {
                this.adorner.Remove();
                this.shouldCreateNewAdorner = true;
            }
        }


    }
}
