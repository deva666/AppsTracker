using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Repos
{
    public sealed class LogRepository : BaseRepository<Log>, IRepository<Log>
    {
        private static Lazy<LogRepository> _instance = new Lazy<LogRepository>(() => new LogRepository());
        public static LogRepository Instance
        {
            get { return _instance.Value; }
        }

        private LogRepository() { }

        public override Log GetByID(int id)
        {
            throw new NotImplementedException();
        }
    }
}
