using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Repos
{
    public sealed class RepositoryFactory : IRepositoryFactory
    {
        private static Lazy<RepositoryFactory> _instance = new Lazy<RepositoryFactory>(() => new RepositoryFactory());
        public static RepositoryFactory Instance
        {
            get { return _instance.Value; }
        }

        private readonly Hashtable _hashTable = new Hashtable();

        private RepositoryFactory()
        {
            Bind<IRepository<Log>>(() => LogRepository.Instance);
            Bind<IEditableRepository<Log>>(() => LogEditableRepository.Instance);
        }

        private void Bind<T>(Func<T> getter) where T : class
        {
            if (getter == null)
                throw new ArgumentNullException("Getter");
            if (_hashTable.ContainsKey(typeof(T)))
                throw new InvalidOperationException(string.Format("{0} is already inserted", typeof(T)));
            _hashTable.Add(typeof(T), getter);
        }

        public T Get<T>() where T : class
        {
            if (!_hashTable.ContainsKey(typeof(T)))
                throw new InvalidOperationException(string.Format("Can't resolve {0}", typeof(T)));
            var getter = _hashTable[typeof(T)];
            Func<T> res = (Func<T>)getter;
            return res();
        }

    }
}
