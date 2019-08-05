using System;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Utilities
{
    public interface IAsyncHelper
    {
        void Synchronise(Func<Task> asyncFunction);

        T Synchronise<T>(Func<Task<T>> asyncFunction);
    }
}