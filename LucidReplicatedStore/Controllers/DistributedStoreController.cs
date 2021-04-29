using LucidBase.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace LucidStore.Controllers
{
    [ApiController]
    [Route("")]
    public class DistributedStoreController : ControllerBase
    {
        private readonly IReplicatedStateMachine _replicated;

        public DistributedStoreController(IReplicatedStateMachine replicated)
        {
            _replicated = replicated;
        }

        [HttpGet]
        [Produces("text/plain")]
        [Route("{key}")]
        public ValueTask<string> GetAsync(string key)
        {
            return _replicated.GetAsync(key);
        }

        [HttpPost]
        [Route("test/{key}")]
        public async ValueTask SwaggerPostAsync(string key, [FromBody] string data)
        {
            await _replicated.SetAsync(key, data);
        }

        [HttpPost]
        [Route("{key}")]
        public async ValueTask PostAsync(string key)
        {
            string data;
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                data = await reader.ReadToEndAsync();
            }

            await _replicated.SetAsync(key, data);
        }
    }
}
