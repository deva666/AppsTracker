using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Domain.Utils;
using AppsTracker.Domain.Logs;
using AppsTracker.Domain.Model;
using AppsTracker.Data.Models;

namespace AppsTracker.Domain.Screenshots
{
    public sealed class ScreenshotModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal ScreenshotModel(LogModel log, Screenshot screenshot)
        {
            Log = log;
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
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
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
            get
            {
                return this.Log.Window.Application.Name;
            }
        }

        private byte[] GetByteArrayFromImage(Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        private void GetPopupSize()
        {
            //double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            //double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            //if (this.Width > this.Height && this.Height >= screenHeight - 100d)
            //{
            //    this.PopupWidth = screenWidth * 0.75;
            //    this.PopupHeight = screenHeight;
            //}
            //else if (this.Width < this.Height && this.Width >= screenWidth - 150d)
            //{
            //    this.PopupHeight = screenHeight * 0.75;
            //    this.PopupWidth = screenWidth;
            //}
            //else
            //{
            //    this.PopupWidth = Width;
            //    this.PopupHeight = Height;
            //}
        }

        private void ShowHide()
        {
            if (IsOpen)
                IsOpen = false;
            else
                IsOpen = true;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("IsOpen"));
        }


        public DateTime Date { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public byte[] Screensht { get; set; }

        public double PopupHeight { get; set; }

        public double PopupWidth { get; set; }

        public LogModel Log { get; set; }
    }
}
