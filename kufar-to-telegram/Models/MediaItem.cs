using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace kufar_to_telegram.Models
{
    public class MediaItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "photo";

        [JsonPropertyName("media")]
        public string Media { get; set; } = default!;

        [JsonPropertyName("caption")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Caption { get; set; }
    }
}
