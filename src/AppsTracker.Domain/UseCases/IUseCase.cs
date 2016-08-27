using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Domain.UseCases
{
    public interface IUseCase<T>
    {
        IEnumerable<T> Get();
    }
}
