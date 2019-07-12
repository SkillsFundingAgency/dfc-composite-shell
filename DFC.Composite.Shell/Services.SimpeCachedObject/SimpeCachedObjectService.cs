using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.SimpeCachedObject
{
    public class SimpeCachedObjectService<T> : ISimpeCachedObjectService<T>
    {
        private T _cachedObject;
        private DateTime _cacheDateTime;
        private readonly object _cacheLock = new object();

        public int CacheDurationInSeconds { get; set; } = 10;

        public T CachedObject
        {
            get
            {
                var diffInSeconds = (DateTime.Now - _cacheDateTime).TotalSeconds;

                if (diffInSeconds > CacheDurationInSeconds)
                {
                    CachedObject = default(T);
                }

                return _cachedObject;
            }
            set
            {
                lock (_cacheLock)
                {
                    _cachedObject = value;

                    _cacheDateTime = DateTime.Now;
                }
            }
        }
    }
}
