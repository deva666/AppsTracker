using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppsTracker.Domain
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

    public interface IUseCase<in T1In, in T2In, in T3In, out TOut>
    {
        IEnumerable<TOut> Get(T1In arg1, T2In arg2, T3In arg3);
    }

    public interface IUseCaseAsync<T>
    {
        Task<IEnumerable<T>> GetAsync();
    }
}
