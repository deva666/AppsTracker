using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.Logging;
using Task_Logger_Pro.Models;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Data_filelogsViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        bool _working;

        WeakReference _weakCollection = new WeakReference(null);

        ICommand _deleteFileLogsCommand;


        #region Properties

        public string Title
        {
            get
            {
                return "FILELOGS";
            }
        }
        public bool IsContentLoaded
        {
            get
            {
                return _weakCollection.Target != null;
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
                _working = value; PropertyChanging("Working");
            }
        }

        public WeakReference WeakCollection
        {
            get
            {
                if (_weakCollection.Target == null && !Working)
                    LoadContent();
                return _weakCollection;
            }
        }

        public ICommand DeleteFileLogsCommand
        {
            get
            {
                if (_deleteFileLogsCommand == null)
                    _deleteFileLogsCommand = new DelegateCommand(DeleteFileLogs);
                return _deleteFileLogsCommand;
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        public Data_filelogsViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(() => { if (this.WeakCollection.Target != null) LoadContent(); }));
        }

        public async void LoadContent()
        {
            Working = true;
            WeakCollection.Target = await LoadContentAsync();
            Working = false;
            PropertyChanging("WeakCollection");
        }

        private Task<List<FileLog>> LoadContentAsync()
        {
            return Task<List<FileLog>>.Run(() =>
            {
                using (var context = new AppsEntities1())
                {
                    return (from u in context.Users.AsNoTracking()
                            join f in context.FileLogs.AsNoTracking() on u.UserID equals f.UserID
                            where u.UserID == Globals.SelectedUserID
                            && f.Date >= Globals.Date1
                            && f.Date <= Globals.Date2
                            select f).ToList();
                }
            });
        }

        #region Command Methods

        private async void DeleteFileLogs(object parameter)
        {
            if (parameter == null)
                return;
            ObservableCollection<object> parameters = parameter as ObservableCollection<object>;
            List<FileLog> fileLogsList = parameters.Select(p => (FileLog)p).ToList();
            using (var context = new AppsEntities1())
            {
                foreach (var fileLog in fileLogsList)
                {
                    if (!context.FileLogs.Local.Any(f => f.FileLogID == fileLog.FileLogID))
                        context.FileLogs.Attach(fileLog);
                    context.FileLogs.Remove(fileLog);
                }
                await context.SaveChangesAsync();
            }
            if (fileLogsList.Count > 0)
                LoadContent();

        }

        #endregion

    }
}
