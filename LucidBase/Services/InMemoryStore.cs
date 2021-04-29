using LucidBase.Core.Interfaces;
using LucidBase.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LucidBase.Services
{
    public class InMemoryStore : IStore
    {
        private int _clock;
        private readonly Dictionary<string, Book> _cache;

        public InMemoryStore()
        {
            _clock = 0;
            _cache = new Dictionary<string, Book>();
        }

        public ValueTask<Book> GetBookAsync(string key)
        {
            return new ValueTask<Book>(_cache.GetValueOrDefault(key, new Book()));
        }

        public void SetBook(string key, Book book)
        {
            _cache[key] = book;
        }

        public ValueTask<int> GetClockAsync()
        {
            return new ValueTask<int>(_clock);
        }

        public void SetClock(int clock)
        {
            _clock = clock;
        }
    }
}
