using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class Aplication
    {
        public TimeSpan Duration { get { return this.GetAppDuration( ); } }

        public Aplication(string name)
        {
            this.Name = name;
            this.FileName = "";
            this.Version = "";
            this.Description = "";
            this.Company = "";
            this.WinName = "";
        }

        public Aplication( string name, string fileName, string version, string description,  string company, string realName )
        {
            this.Name = name;
            this.FileName = fileName;
            this.Version = version;
            this.Description = description;
            this.Company = company;
            this.WinName = realName;
        }

    }
}
