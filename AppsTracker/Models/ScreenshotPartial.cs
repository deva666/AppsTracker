using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Models
{
    public partial class Screenshot : INotifyPropertyChanged
    {

        public bool IsOpen
        {
            get;
            set;
        }
        public ICommand ShowHideCommand
        {
            get
            {
                return new DelegateCommand(ShowHide);
            }
        }
        public string AppName
        {
            get
            {
                return this.Log.Window.Application.Name;
            }
        }
        public int UserID
        {
            get
            {
                return this.Log.Window.Application.UserID;
            }
        }
  

        public Screenshot() { }

        public Screenshot(Size size, Image image)
        {
            this.Date = DateTime.Now;
            this.Width = size.Width;
            this.Height = size.Height;
            this.Screensht = GetByteArrayFromImage(image);
            GetPopupSize();
        }



        private byte[] GetByteArrayFromImage(System.Drawing.Image imageIn)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(imageIn, typeof(byte[]));
        }

        private void GetPopupSize()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("IsOpen"));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
