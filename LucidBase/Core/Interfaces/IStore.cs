using LucidBase.Core.Models;
using System.Threading.Tasks;

namespace LucidBase.Core.Interfaces
{
    public interface IStore
    {
        public ValueTask<Book> GetBookAsync(string key);
        public void SetBook(string key, Book book);
        public ValueTask<int> GetClockAsync();
        public void SetClock(int clock);
    }
}
