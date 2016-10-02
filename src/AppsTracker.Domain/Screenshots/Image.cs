using System;
using System.ComponentModel;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Model;
using AppsTracker.Domain.Utils;

namespace AppsTracker.Domain.Screenshots
{
    public sealed class Image : ObservableObject
    {
        private const Double HEIGHT_MARGIN = 100d;
        private const Double WIDTH_MARGIN = 100d;
        private const Double SCALE_FACTOR = 0.75;

        internal Image(String appName, Screenshot screenshot)
        {
            AppName = appName;
            ScreenshotId = screenshot.ID;
            Width = screenshot.Width;
            Height = screenshot.Height;
            Date = screenshot.Date;
            Screensht = screenshot.Screensht;
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetPropertyValue(ref isSelected, value); }
        }

        private bool isOpen;

        public bool IsOpen
        {
            get { return IsOpen; }
            set { SetPropertyValue(ref isOpen, value); }
        }

        public ICommand ShowHideCommand
        {
            get
            {
                return new RelayCommand(ShowHide);
            }
        }

        public string AppName
        {
            get;
        }

        public void SetPopupSize(double screenWidth, double screenHeight)
        {
            if (this.Width > this.Height && this.Height >= screenHeight - HEIGHT_MARGIN)
            {
                this.PopupWidth = screenWidth * SCALE_FACTOR;
                this.PopupHeight = screenHeight;
            }
            else if (this.Width < this.Height && this.Width >= screenWidth - WIDTH_MARGIN)
            {
                this.PopupHeight = screenHeight * SCALE_FACTOR;
                this.PopupWidth = screenWidth;
            }
            else
            {
                this.PopupWidth = Width;
                this.PopupHeight = Height;
            }
        }

        private void ShowHide()
        {
            if (IsOpen)
                IsOpen = false;
            else
                IsOpen = true;
        }

        public Int32 ScreenshotId { get; }

        public DateTime Date { get; }

        public int Width { get; }

        public int Height { get; }

        public byte[] Screensht { get; }

        public double PopupHeight { get; private set; }

        public double PopupWidth { get; private set; }
    }
}
