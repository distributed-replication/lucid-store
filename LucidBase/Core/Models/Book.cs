using System.Collections.Generic;

namespace LucidBase.Core.Models
{
    public class Book
    {
        public List<Decree> Decrees { get; set; }
        public int LatestCommittedIndex { get; set; }
        public string LatestCommittedValue { get; set; }

        public Book()
        {
            Decrees = new List<Decree>();
            LatestCommittedIndex = -1;
        }
    }
}
