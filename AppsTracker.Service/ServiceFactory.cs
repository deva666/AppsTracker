using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Service
{
    public sealed class ServiceFactory
    {
        private static Lazy<ServiceFactory> _instance = new Lazy<ServiceFactory>(() => new ServiceFactory());
        public static ServiceFactory Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private readonly Hashtable _hashTable = new Hashtable();

        private ServiceFactory()
        {
            Bind<IAppsService>(() => new AppsService());
        }

        private void Bind<T>(Func<T> getter) where T : class, IBaseService
        {
            if (getter == null)
                throw new ArgumentNullException("Getter");
            if (_hashTable.ContainsKey(typeof(T)))
                throw new InvalidOperationException(string.Format("{0} is already inserted", typeof(T)));
            _hashTable.Add(typeof(T), getter);
        }

        public T GetService<T>() where T : class, IBaseService
        {
            if (!_hashTable.ContainsKey(typeof(T)))
                throw new InvalidOperationException(string.Format("Can't resolve {0}", typeof(T)));
            var getter = _hashTable[typeof(T)];
            Func<T> res = (Func<T>)getter;
            return res();
        }
    }
}
