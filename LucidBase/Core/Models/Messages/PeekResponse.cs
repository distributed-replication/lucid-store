using System.Text.Json.Serialization;

namespace LucidBase.Core.Models.Messages
{
    public class PeekResponse
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("latestDecree")]
        public Decree LatestDecree { get; set; }
    }
}
