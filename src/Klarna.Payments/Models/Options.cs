using System.Text.Json.Serialization;

namespace Klarna.Payments.Models
{
    public class Options
    {
        [JsonPropertyName("color_border")]
        public string ColorBorder { get; set; }

        [JsonPropertyName("color_border_selected")]
        public string ColorBorderSelected { get; set; }

        [JsonPropertyName("color_text")]
        public string ColorText { get; set; }

        [JsonPropertyName("color_details")]
        public string ColorDetails { get; set; }

        [JsonPropertyName("radius_border")]
        public string RadiusBorder { get; set; }
    }
}