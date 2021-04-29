using LucidBase.Core.Models.Messages;
using System.Threading.Tasks;

namespace LucidBase.Core.Interfaces
{
    public interface IReplicatedStateMachine
    {
        public ValueTask<PassDecreeResponse> PassDecreeAsync(string key, int clock, int index, string value);
        public ValueTask<bool> CommitDecreeAsync(string key, int index, string value);
        public ValueTask<PeekResponse> PeekRequestAsync(string key);
        public ValueTask<string> GetAsync(string key);
        public ValueTask SetAsync(string key, string value);
    }
}
