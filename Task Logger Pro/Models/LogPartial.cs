using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class Log : INotifyPropertyChanged
    {
        private bool _finished = false;

        public bool Finished { get { return _finished; } }
        public string AppName { get { return this.Window.Application.Name; } }

        public string WindowTitle { get { return this.Window.Title; } }

        public long Duration
        {
            get
            {
                return  DateEnded.Ticks - DateCreated.Ticks;
            }
        }

        bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        public bool HasKeystrokes
        {
            get
            {
                return this.KeystrokesRaw.Count() == 0 ? false : true;
            }
        }

        public int UserID
        {
            get
            {
                return this.Window.Application.UserID;
            }
        }

        private StringBuilder stringBuilder;

        public StringBuilder StringBuilder
        {
            get
            {
                if (stringBuilder == null)
                    stringBuilder = new StringBuilder();
                return stringBuilder;
            }
        }

        private StringBuilder stringBuilderRaw;

        public StringBuilder StringBuilderRaw
        {
            get
            {
                if (stringBuilderRaw == null)
                    stringBuilderRaw = new StringBuilder();
                return stringBuilderRaw;
            }
        }

        public Log(int windowID)
            : this()
        {
            this.WindowID = windowID;
            this.DateCreated = DateTime.Now;
        }

        public Log(int windowID, int usageID)
            : this(windowID)
        {
            this.UsageID = usageID;
        }

        public void Finish()
        {
            if (!_finished)
            {
                _finished = true;
                DateEnded = DateTime.Now;

                if (stringBuilder != null)
                    this.Keystrokes = stringBuilder.ToString();
                if (stringBuilderRaw != null)
                    this.KeystrokesRaw = stringBuilderRaw.ToString();
            }
            else
            {
                Debug.Fail("Log finish called twice!");
            }
        }

        public void AppendKeyLog(string str)
        {
            this.StringBuilder.Append(str);
        }

        public void AppendKeyLogRaw(string keyName)
        {
            this.StringBuilderRaw.Append(keyName);
        }

        public void RemoveLastKeyLogItem()
        {
            if (stringBuilder != null)
                if (stringBuilder.Length > 0)
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }

        public void AppendNewKeyLogLine()
        {
            this.StringBuilder.AppendLine();
            this.StringBuilderRaw.AppendLine();
        }

        public void AppendSpace()
        {
            this.StringBuilderRaw.Append(" ");
        }

        public void AppendTab()
        {
            this.StringBuilderRaw.Append("\t");
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
