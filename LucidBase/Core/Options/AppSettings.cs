using System.Collections.Generic;

namespace LucidBase.Options
{
    public class AppSettings
    {
        public List<string> NetworkAddresses { get; set; }
        public List<string> Ids { get; set; }
        public string NodeId { get; set; }
        public int QuorumSize { get; set; }
        public int LucidHttpTimeout { get; set; }
    }
}
