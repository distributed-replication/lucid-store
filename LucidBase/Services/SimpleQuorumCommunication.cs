using LucidBase.Core.Interfaces;
using LucidBase.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LucidBase.Services
{
    public class SimpleQuorumCommunication : ICommunicate
    {
        private readonly List<HttpClient> _clients;
        private readonly int _quorumSize;
        private readonly Random _random;

        public SimpleQuorumCommunication(IOptionsMonitor<AppSettings> appSettingsOptions)
        {
            var appSettings = appSettingsOptions.CurrentValue;

            _quorumSize = appSettings.QuorumSize;
            _clients = new List<HttpClient>();
            _random = new Random();
            for (int i = 0; i <= appSettings.NetworkAddresses.Count - 1; i++)
                if (appSettings.Ids[i] != appSettings.NodeId)
                    _clients.Add(new HttpClient()
                    {
                        BaseAddress = new Uri(appSettings.NetworkAddresses[i]),
                        Timeout = TimeSpan.FromSeconds(appSettings.LucidHttpTimeout)
                    });
        }

        public async Task<List<T>> Broadcast<T>(string url)
        {
            T[] responses = new T[_clients.Count];
            var possiblyFailedReplicas = new List<int>();

            do
            {
                var i = _random.Next(_clients.Count);

                if (responses[i] == null && !possiblyFailedReplicas.Contains(i))
                {
                    try
                    {
                        responses[i] = JsonSerializer.Deserialize<T>(await _clients[i].GetStringAsync(url));
                    }
                    catch (Exception)
                    {
                        possiblyFailedReplicas.Add(i);

                        if (possiblyFailedReplicas.Count == _clients.Count)
                            possiblyFailedReplicas.Clear();
                    }
                }
            } while (!QuorumReached(responses));

            return responses.Where(r => r != null)
                            .ToList();
        }

        private bool QuorumReached<T>(T[] responses)
        {
            return responses.Where(r => r != null).Count() >= _quorumSize;
        }
    }
}
