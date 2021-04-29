using System.Text.Json.Serialization;

namespace LucidBase.Core.Models.Messages
{
    public class PassDecreeResponse
    {
        [JsonPropertyName("clock")]
        public int Clock { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("maxIndexSoFar")]
        public int MaxIndexSoFar { get; set; }
    }
}
