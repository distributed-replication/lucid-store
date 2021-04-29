using LucidBase.Core.Interfaces;
using LucidBase.Domain.Lucid.Services;
using LucidBase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LucidStore.Extensions
{
    public static class WiringExt
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<IStore, InMemoryStore>();
            services.AddSingleton<ICommunicate, SimpleQuorumCommunication>();
            services.AddSingleton<IReplicatedStateMachine, LucidReplicatedStateMachine>();
        }
    }
}
