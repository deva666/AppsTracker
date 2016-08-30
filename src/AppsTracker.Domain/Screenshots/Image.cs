using System;
using System.ComponentModel;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Utils;

namespace AppsTracker.Domain.Screenshots
{
    public sealed class Image : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
            set
            {
                isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        public bool IsOpen
        {
            get;
            set;
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
            if (this.Width > this.Height && this.Height >= screenHeight - 100d)
            {
                this.PopupWidth = screenWidth * 0.75;
                this.PopupHeight = screenHeight;
            }
            else if (this.Width < this.Height && this.Width >= screenWidth - 150d)
            {
                this.PopupHeight = screenHeight * 0.75;
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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOpen"));
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
