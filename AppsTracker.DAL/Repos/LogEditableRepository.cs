using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Repos
{
    public sealed class LogEditableRepository : BaseEditableRepository<Log>, IEditableRepository<Log>
    {
        private static Lazy<LogEditableRepository> _instance = new Lazy<LogEditableRepository>(() => new LogEditableRepository());
        public static LogEditableRepository Instance
        {
            get { return _instance.Value; }
        }
    }
}
