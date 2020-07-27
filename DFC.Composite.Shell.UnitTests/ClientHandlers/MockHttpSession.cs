using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.UnitTests.ClientHandlers
{
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

        string ISession.Id => throw new NotImplementedException();

        bool ISession.IsAvailable => throw new NotImplementedException();

        IEnumerable<string> ISession.Keys => sessionStorage.Keys;

        void ISession.Clear()
        {
            sessionStorage.Clear();
        }

        Task ISession.CommitAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ISession.LoadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void ISession.Remove(string key)
        {
            sessionStorage.Remove(key);
        }

        void ISession.Set(string key, byte[] value)
        {
            sessionStorage[key] = Encoding.UTF8.GetString(value);
        }

        bool ISession.TryGetValue(string key, out byte[] value)
        {
            if (sessionStorage.Any(x => x.Key == key))
            {
                value = Encoding.ASCII.GetBytes(sessionStorage[key].ToString());
                return true;
            }

            value = null;
            return false;
        }
    }
}
