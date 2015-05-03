using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Hooks
{
    public sealed class MouseHookArgs : EventArgs
    {
        Point _point;
        int _mouseButton;

        public Point Point { get { return _point; } }
        public MouseButton MouseButton
        {
            get
            {
                if (_mouseButton == MouseHook.WM_LBUTTONDOWN)
                {
                    return MouseButton.LeftDown;
                }
                else if (_mouseButton == MouseHook.WM_RBUTTONDOWN)
                {
                    return MouseButton.RightDown;
                }
                else if (_mouseButton == MouseHook.WM_LBUTTONUP)
                {
                    return MouseButton.LeftUp;
                }
                else if (_mouseButton == MouseHook.WM_RBUTTONUP)
                {
                    return MouseButton.RightUp;
                }
                else
                    return MouseButton.None;
            }
        }

        public MouseHookArgs(Point point, int mouseButton)
        {
            _point = point;
            _mouseButton = mouseButton;
        }

    }
}
