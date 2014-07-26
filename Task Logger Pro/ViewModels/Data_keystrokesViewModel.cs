using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.Logging;
using Task_Logger_Pro.MVVM;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Data_keystrokesViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        List<Log> _logList;


        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "KEYSTROKES";
            }
        }
        public bool IsContentLoaded
        {
            get;
            private set;
        }
        public List<Log> LogList
        {
            get
            {
                return _logList;
            }
            set
            {
                _logList = value;
                PropertyChanging("LogList");
            }
        }

        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;
                PropertyChanging("Working");
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        public Data_keystrokesViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        public async void LoadContent()
        {
            Working = true;
            LogList = await GetContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<Log>> GetContentAsync()
        {
            return Task<List<Log>>.Run(new Func<List<Log>>(GetContent));
        }

        private List<Log> GetContent()
        {
            using (var context = new AppsEntities())
            {
                return (from u in context.Users.AsNoTracking()
                        join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                        join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                        join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                        where u.UserID == Globals.SelectedUserID
                        && l.KeystrokesRaw != null
                        && l.DateCreated >= Globals.Date1
                        && l.DateCreated <= Globals.Date2
                        select l).Include(l => l.Window.Application)
                                                  .Include(l => l.Screenshots)
                                                  .ToList();

            }
        }

        private Task<List<Log>> LoadContentAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var context = new AppsEntities())
                {
                    return (from u in context.Users.AsNoTracking()
                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                            where u.UserID == Globals.SelectedUserID
                            && l.KeystrokesRaw != null
                            && l.DateCreated >= Globals.Date1
                            && l.DateCreated <= Globals.Date2
                            select l).Include(l => l.Window.Application)
                                                      .Include(l => l.Screenshots)
                                                      .ToList();

                }
            });
        }
    }
}
