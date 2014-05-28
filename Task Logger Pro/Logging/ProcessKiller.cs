using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Data.Entity;
//using Task_Logger_Pro.Models;
using System.Threading.Tasks;
using System.IO;
using AppsTracker.Models.EntityModels;
using AppsTracker.DAL;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Logging
{
    public sealed class ProcessKiller : IDisposable, ICommunicator
    {
        #region Fields

        ManagementEventWatcher _managementEventWatcher;

        IEnumerable<AppsToBlock> _appsToBlockList;

        #endregion

        #region Properties

        public event EventHandler<ProcessKilledEventArgs> ProcessKilledEvent;

        public IEnumerable<AppsToBlock> AppsToBlockList
        {
            get
            {
                return _appsToBlockList;
            }
            set
            {
                _appsToBlockList = value;
                BlockAsync();
            }
        }

        #endregion

        #region Constructor

        public ProcessKiller()
        {
            Mediator.Register(MediatorMessages.AppsToBlockChanged, new Action<List<AppsToBlock>>(RefreshAppsToBlock));
            WqlEventQuery query = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\"");
            _managementEventWatcher = new ManagementEventWatcher();
            _managementEventWatcher.Query = query;
            _managementEventWatcher.Options.Timeout = new TimeSpan(0, 0, 5);
            _managementEventWatcher.Start();
            _managementEventWatcher.EventArrived += processStartEvent_EventArrived;
        }

        public ProcessKiller(IEnumerable<AppsToBlock> collection)
            : this()
        {
            _appsToBlockList = collection;
        }

        #endregion

        private void Stop()
        {
            _managementEventWatcher.Stop();
            _managementEventWatcher.EventArrived -= processStartEvent_EventArrived;
        }

        public void RefreshAppsToBlock(List<AppsToBlock> appsToBlockList)
        {
            _appsToBlockList = appsToBlockList;
            BlockAsync();
        }

        #region Event Handlers

        private void processStartEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            BlockAsync();
        }

        #endregion

        #region Class Methods

        private Task BlockAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var blockedApp in _appsToBlockList)
                    {
                        if (!IsProcessKilled(blockedApp))
                            continue;
                        Process[] processCollection = Process.GetProcessesByName(blockedApp.Application.WinName);
                        if (processCollection.Length > 0)
                        {
                            foreach (var process in processCollection)
                            {
                                process.Kill();
                            }
                            var handler = ProcessKilledEvent;
                            if (handler != null)
                                handler(this, new ProcessKilledEventArgs(blockedApp.Application));
                        }
                    }
                }
                catch
                {
                    
                }
            });
        }

        private bool IsProcessKilled(AppsToBlock  appsToBlock)
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            switch (today)
            {
                case DayOfWeek.Friday:
                    break;
                case DayOfWeek.Monday:
                    break;
                case DayOfWeek.Saturday:
                    break;
                case DayOfWeek.Sunday:
                    break;
                case DayOfWeek.Thursday:
                    break;
                case DayOfWeek.Tuesday:
                    break;
                case DayOfWeek.Wednesday:
                    break;
                default:
                    break;
            }
            string day = System.Enum.GetName(typeof(DayOfWeek), today);
            switch (day)
            {
                case "Monday":
                    if (appsToBlock.Monday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case "Tuesday":
                    if (appsToBlock.Tuesday && (DateTime.Now.TimeOfDay >=  new TimeSpan( appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case "Wednesday":
                    if (appsToBlock.Wednesday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case "Thursday":
                    if (appsToBlock.Thursday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case "Friday":
                    if (appsToBlock.Friday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case "Saturday":
                    if (appsToBlock.Saturday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case "Sunday":
                    if (appsToBlock.Sunday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                default:
                    break;
            }
            return true;
        }

        #endregion

        #region IDisposable Methods

        private void Dispose(bool disposing)
        {
            Debug.WriteLine("Disposing " + this.GetType()); 
            if (_managementEventWatcher != null)
            {
                this.Stop();
                _managementEventWatcher.Dispose();
                _managementEventWatcher = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }
    }

    public class ProcessKilledEventArgs : EventArgs
    {
        Aplication _aplication;

        public Aplication Aplication
        {
            get { return _aplication; }
        }

        public ProcessKilledEventArgs(Aplication aplication)
        {
            _aplication = aplication;
        }

    }

}
