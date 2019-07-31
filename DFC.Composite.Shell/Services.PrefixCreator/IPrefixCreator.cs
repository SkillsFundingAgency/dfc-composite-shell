using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.PrefixCreator
{
    /// <summary>
    /// Determines how to create prefixes
    /// </summary>
    public interface IPrefixCreator
    {
        string Resolve(Uri uri);
    }
}
