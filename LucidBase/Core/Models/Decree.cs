
using System.Text.Json.Serialization;

namespace LucidBase.Core.Models
{
    public class Decree
    {
        [JsonPropertyName("subjectValue")]
        public string SubjectValue { get; set; }
        [JsonPropertyName("committed")]
        public bool Committed { get; set; }
    }
}
