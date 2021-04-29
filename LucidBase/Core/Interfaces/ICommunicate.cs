using System.Collections.Generic;
using System.Threading.Tasks;

namespace LucidBase.Core.Interfaces
{
    public interface ICommunicate
    {
        public Task<List<T>> Broadcast<T>(string url);
    }
}
