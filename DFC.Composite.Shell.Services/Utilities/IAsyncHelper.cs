using System;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Utilities
{
    public interface IAsyncHelper
    {
        T Synchronise<T>(Func<Task<T>> asyncFunction);
    }
}