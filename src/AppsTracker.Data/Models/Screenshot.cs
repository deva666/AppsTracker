#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Windows.Input;

namespace AppsTracker.Data.Models
{
    public class Screenshot : IEntity
    {
        public Screenshot() { }

        public Screenshot(Size size, Image image)
        {
            this.Date = DateTime.Now;
            this.Width = size.Width;
            this.Height = size.Height;
            this.Screensht = GetByteArrayFromImage(image);
        }

        private byte[] GetByteArrayFromImage(Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ScreenshotID")]
        public int ID { get; set; }

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
