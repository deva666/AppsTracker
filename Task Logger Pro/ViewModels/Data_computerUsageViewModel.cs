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
    class Data_computerUsageViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields
        
        bool _working;

        WeakReference _weakCollection = new WeakReference(null);

        ICommand _deleteLogsCommand; 
        
        #endregion

        #region Properties
        
        public string Title
        {
            get
            {
                return "COMPUTER USAGE";
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
                _working = value;
                PropertyChanging("Working");
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

        public ICommand DeleteLogsCommand
        {
            get
            {
                _deleteLogsCommand = new DelegateCommand(DeleteLogs);
                return _deleteLogsCommand;
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        public Data_computerUsageViewModel()
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

        private Task<List<Usage>> LoadContentAsync( )
        {
            return Task<List<Usage>>.Run( ( ) =>
            {
                using ( var context = new AppsEntities1( ) )
                {
                    string loginType = UsageTypes.Login.ToString();

                    return ( from u in context.Users.AsNoTracking( )
                             join l in context.Usages.AsNoTracking( )
                             on u.UserID equals l.UserID
                             where u.UserID == Globals.SelectedUserID
                             && l.UsageStart >= Globals.Date1
                             && (l.UsageEnd<= Globals.Date2 || l.UsageEnd == null)
                             && l.UsageType.UType == loginType
                             select l ).ToList( );

                }
            } );
        }

        private void DeleteLogs(object parameter)
        {
            if (parameter == null)
                return;
            ObservableCollection<object> parameterCollection = parameter as ObservableCollection<object>;
            if (parameterCollection != null)
            {
                var logins = parameterCollection.Select( l => l as Usage ).ToList( );
                using ( var context = new AppsEntities1( ) )
                {
                    foreach ( var login in logins )
                    {
                        if ( !context.Usages.Local.Any( l => l.UsageID == login.UsageID) )
                        {
                            context.Usages.Attach( login );
                        }
                        context.Usages.Remove( login );
                    }
                    context.SaveChanges( );
                }
            }

            LoadContent();
        }

    }
}
