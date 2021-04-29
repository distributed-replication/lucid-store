using LucidBase.Core.Interfaces;
using LucidBase.Core.Models;
using LucidBase.Core.Models.Messages;
using LucidBase.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace LucidBase.Domain.Lucid.Services
{
    public class LucidReplicatedStateMachine : IReplicatedStateMachine
    {
        private readonly IMemoryCache _store;
        private readonly ICommunicate _communicate;
        private readonly LucidUrls _urls;
        private const string ClockKey = "#clock#";

        public LucidReplicatedStateMachine(IMemoryCache store, ICommunicate communicate, IOptionsMonitor<LucidUrls> urls)
        {
            _store = store;
            _communicate = communicate;
            _urls = urls.CurrentValue;
        }

        public async ValueTask<PassDecreeResponse> PassDecreeAsync(string key, int clock, int index, string value)
        {
            var processClock = await _store.GetOrCreateAsync(ClockKey, a => { return Task.FromResult(0); });

            Book book = await _store.GetOrCreateAsync(key, a => { return Task.FromResult(new Book()); });

            var decrees = book.Decrees;

            var response = new PassDecreeResponse()
            {
                Index = index,
                MaxIndexSoFar = decrees.Count - 1
            };

            response.Clock = clock;

            if (decrees.Count >= index + 1)
            {
                if (decrees[index] != null)
                {
                    response.Value = decrees[index].SubjectValue;
                }
                else
                {
                    response.Value = value;
                    decrees[index] = new Decree()
                    {
                        Committed = false,
                        SubjectValue = value
                    };
                }
            }
            else
            {
                response.Value = value;

                for (int i = decrees.Count - 1; i < index - 1; i++)
                {
                    decrees.Add(null);
                }

                decrees.Add(new Decree()
                {
                    Committed = false,
                    SubjectValue = value
                });
            }

            _store.Set(ClockKey, Math.Max(clock, processClock) + 1);

            return response;
        }

        public async ValueTask<bool> CommitDecreeAsync(string key, int index, string value)
        {
            var book = await _store.GetOrCreateAsync(key, a => { return Task.FromResult(new Book()); });

            var decrees = book.Decrees;

            if (decrees.Count < index + 1)
            {
                for (int i = decrees.Count - 1; i < index - 1; i++)
                {
                    decrees.Add(null);
                }

                decrees.Add(new Decree()
                {
                    Committed = true,
                    SubjectValue = value
                });
            }
            else
            {
                decrees[index] = new Decree()
                {
                    Committed = true,
                    SubjectValue = value
                };
            }

            if (index > book.LatestCommittedIndex)
                book.LatestCommittedIndex = index;

            book.LatestCommittedValue = decrees[book.LatestCommittedIndex].SubjectValue;

            return await Task.FromResult(true);
        }

        public async ValueTask<PeekResponse> PeekRequestAsync(string key)
        {
            var book = await _store.GetOrCreateAsync(key, a => { return Task.FromResult(new Book()); });

            var index = book.Decrees.Count - 1;

            var response = new PeekResponse()
            {
                Key = key,
                Index = index,
                LatestDecree = index - 1 >= 0 ? new Decree()
                {
                    Committed = book.Decrees[index].Committed,
                    SubjectValue = book.Decrees[index].SubjectValue
                } : null
            };

            return response;
        }

        public async ValueTask<string> GetAsync(string key)
        {
            if (key.Contains("#"))
                throw new Exception("'#' is a reserved character.");

            var book = await _store.GetOrCreateAsync(key, a => { return Task.FromResult(new Book()); });

            var peekResponses = await _communicate.Broadcast<PeekResponse>(string.Format(_urls.Peek, key));

            int index = book.LatestCommittedIndex;
            string value = null;
            bool committed = false;

            foreach (var response in peekResponses)
            {
                var decree = response.LatestDecree;

                if (decree == null)
                    continue;

                if (response.Index > index)
                {
                    index = response.Index;
                    value = decree.SubjectValue;
                    committed = decree.Committed;
                }

                if (response.Index == index && decree.Committed)
                {
                    committed = decree.Committed;
                    value = decree.SubjectValue;
                }
            }

            if (index > book.LatestCommittedIndex)
            {
                if (committed)
                    await CommitDecreeAsync(key, index, value);
                else
                    await SetAsync(key, value);
            }
            else if (index == book.LatestCommittedIndex)
            {
                value = book.LatestCommittedValue;
            }

            return value;
        }

        public async ValueTask SetAsync(string key, string value)
        {
            var processClock = await _store.GetOrCreateAsync(ClockKey, a => { return Task.FromResult(0); });

            if (key.Contains("#"))
                throw new Exception("'#' is a reserved character.");

            var book = await _store.GetOrCreateAsync(key, a => { return Task.FromResult(new Book()); });

            var decrees = book.Decrees;
            int maxIndex = decrees.Count - 1;

            bool canFinalize;
            do
            {
                processClock++;
                maxIndex++;
                canFinalize = true;

                var passDecreeResponses = await _communicate.Broadcast<PassDecreeResponse>(string.Format(_urls.PassDecree, key, processClock, maxIndex, value));

                foreach (var response in passDecreeResponses)
                {
                    if (response.Clock > processClock)
                        processClock = response.Clock;

                    if (response.MaxIndexSoFar > maxIndex)
                        maxIndex = response.MaxIndexSoFar;

                    if (response.Value != value)
                        canFinalize = false;
                }
            } while (!canFinalize);

            _store.Set(ClockKey, processClock);

            await CommitDecreeAsync(key, maxIndex, value);
        }
    }
}
