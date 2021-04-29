using LucidBase.Core.Interfaces;
using LucidBase.Core.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LucidStore.Controllers
{
    [ApiController]
    [Route("replicatedStateMachine")]
    public class ReplicatedStateMachineController : ControllerBase
    {
        private readonly IReplicatedStateMachine _replicated;

        public ReplicatedStateMachineController(IReplicatedStateMachine replicated)
        {
            _replicated = replicated;
        }

        [HttpGet]
        [Route("passDecree/{key}/{clock}/{index}/{value}")]
        public ValueTask<PassDecreeResponse> PassDecreeAsync(string key, int clock, int index, string value)
        {
            return _replicated.PassDecreeAsync(key, clock, index, value);
        }

        [HttpGet]
        [Route("commitDecree/{key}/{index}/{value}")]
        public ValueTask<bool> CommitDecreeAsync(string key, int index, string value)
        {
            return _replicated.CommitDecreeAsync(key, index, value);
        }

        [HttpGet]
        [Route("peek/{key}")]
        public ValueTask<PeekResponse> PeekRequestAsync(string key)
        {
            return _replicated.PeekRequestAsync(key);
        }
    }
}
