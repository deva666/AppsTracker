using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppsTracker.Widgets
{
    internal static class UIHelper
    {
        /// <summary>
        /// Get the UIElement that is in the container at the point specified
        /// </summary>
        /// <param name="container"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        internal static UIElement GetUIElement(ItemsControl container, Point position)
        {
            UIElement elementAtPosition = container.InputHitTest(position) as UIElement;
            //move up the UI tree until you find the actual UIElement that is the Item of the container
            if (elementAtPosition != null)
            {
                while (elementAtPosition != null)
                {
                    object testUIElement = container.ItemContainerGenerator.ItemFromContainer(elementAtPosition);
                    if (testUIElement != DependencyProperty.UnsetValue) //if found the UIElement
                    {
                        return elementAtPosition;
                    }
                    else
                    {
                        elementAtPosition = VisualTreeHelper.GetParent(elementAtPosition) as UIElement;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Determines if the relative position is above the UIElement in the coordinate
        /// </summary>
        /// <param name="i"></param>
        /// <param name="relativePosition"></param>
        /// <returns></returns>
        internal static bool IsPositionAboveElement(UIElement i, Point relativePosition)
        {
            if (relativePosition != null)
                if (relativePosition.Y < ((FrameworkElement)i).ActualHeight / 2) //if above
                    return true;
            return false;
        }
    }
}
