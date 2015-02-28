#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Windows.Input;
using AppsTracker.Data.Utils;

namespace AppsTracker.Data.Models
{
    public class Screenshot : INotifyPropertyChanged
    {
        [NotMapped]
        public bool IsOpen
        {
            get;
            set;
        }

        [NotMapped]
        public ICommand ShowHideCommand
        {
            get
            {
                return new DelegateCommand(ShowHide);
            }
        }

        [NotMapped]
        public string AppName
        {
            get
            {
                return this.Log.Window.Application.Name;
            }
        }

        [NotMapped]
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



        private byte[] GetByteArrayFromImage(Image imageIn)
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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScreenshotID { get; set; }

        [Required]
        public System.DateTime Date { get; set; }

        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public int LogID { get; set; }

        [Required]
        [Column(TypeName = "image")]
        [MaxLength]
        public byte[] Screensht { get; set; }

        [Required]
        public double PopupHeight { get; set; }

        [Required]
        public double PopupWidth { get; set; }

        [ForeignKey("LogID")]
        public virtual Log Log { get; set; }
    }
}
