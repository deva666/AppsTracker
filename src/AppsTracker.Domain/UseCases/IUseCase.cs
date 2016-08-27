using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Domain.UseCases
{
    public interface IUseCase<out T>
    {
        IEnumerable<T> Get();
    }

    public interface IUseCase<TIn, out TOut>
    {
        IEnumerable<TOut> Get(TIn arg);
    }
}
