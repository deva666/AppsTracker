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

    public interface IUseCase<in TIn, out TOut>
    {
        IEnumerable<TOut> Get(TIn arg);
    }

    public interface IUseCase<in T1In, in T2In, out TOut>
    {
        IEnumerable<TOut> Get(T1In arg1, T2In arg2);
    }
}
