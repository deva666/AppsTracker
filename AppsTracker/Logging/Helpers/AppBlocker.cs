using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AppsTracker;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;
using AppsTracker.MVVM;

namespace AppsTracker.Logging
{
    internal sealed class AppBlocker : IComponent, ICommunicator
    {
        #region Fields

        ManagementEventWatcher _managementEventWatcher;

        IEnumerable<AppsToBlock> _appsToBlockList;

        #endregion

        #region Properties

        public event EventHandler<AppBlockerEventArgs> AppBlocked;

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

        public AppBlocker()
        {
            Mediator.Register(MediatorMessages.AppsToBlockChanged, new Action<IList<AppsToBlock>>(RefreshAppsToBlock));
            WqlEventQuery query = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\"");
            _managementEventWatcher = new ManagementEventWatcher();
            _managementEventWatcher.Query = query;
            _managementEventWatcher.Options.Timeout = new TimeSpan(0, 0, 5);
            _managementEventWatcher.Start();
            _managementEventWatcher.EventArrived += processStartEvent_EventArrived;
        }

        public AppBlocker(IEnumerable<AppsToBlock> collection)
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

        public void RefreshAppsToBlock(IList<AppsToBlock> appsToBlockList)
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
        private void Block()
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
                            process.Kill();

                        AppBlocked.InvokeSafely<AppBlockerEventArgs>(this, new AppBlockerEventArgs(blockedApp.Application));
                    }
                }
            }
            catch
            {
            }
        }

        private Task BlockAsync()
        {
            return Task.Run(new Action(Block));
        }

        private bool IsProcessKilled(AppsToBlock appsToBlock)
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            switch (today)
            {
                case DayOfWeek.Friday:
                    if (appsToBlock.Friday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case DayOfWeek.Monday:
                    if (appsToBlock.Monday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case DayOfWeek.Saturday:
                    if (appsToBlock.Saturday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case DayOfWeek.Sunday:
                    if (appsToBlock.Sunday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case DayOfWeek.Thursday:
                    if (appsToBlock.Thursday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case DayOfWeek.Tuesday:
                    if (appsToBlock.Tuesday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
                        return false;
                    break;
                case DayOfWeek.Wednesday:
                    if (appsToBlock.Wednesday && (DateTime.Now.TimeOfDay >= new TimeSpan(appsToBlock.TimeMin) && DateTime.Now.TimeOfDay <= new TimeSpan(appsToBlock.TimeMax)))
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
            if (_managementEventWatcher != null)
            {
                this.Stop();
                _managementEventWatcher.Dispose();
                _managementEventWatcher = null;
            }
        }

        #endregion

        public IMediator Mediator
        {
            get { return AppsTracker.MVVM.Mediator.Instance; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void SettingsChanged(ISettings settings)
        {

        }


        public void SetComponentEnabled(bool enabled)
        {
            
        }
    }

    public class AppBlockerEventArgs : EventArgs
    {
        Aplication _aplication;

        public Aplication Aplication
        {
            get { return _aplication; }
        }

        public AppBlockerEventArgs(Aplication aplication)
        {
            _aplication = aplication;
        }
    }
}
