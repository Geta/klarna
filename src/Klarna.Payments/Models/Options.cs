using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Options
    {
        [JsonProperty("color_button")]
        public string ColorButton { get; set; }

        [JsonProperty("color_button_text")]
        public string ColorButtonText { get; set; }
        [JsonProperty("color_checkbox")]
        public string ColorCheckbox { get; set; }
        [JsonProperty("color_checkbox_checkmark")]
        public string ColorCheckboxCheckmark { get; set; }
        [JsonProperty("color_header")]
        public string ColorHeader { get; set; }
        [JsonProperty("color_link")]
        public string ColorLink { get; set; }
        [JsonProperty("color_border")]
        public string ColorBorder { get; set; }
        [JsonProperty("color_border_selected")]
        public string ColorBorderSelected { get; set; }
        [JsonProperty("color_text")]
        public string ColorText { get; set; }
        [JsonProperty("color_details")]
        public string ColorDetails { get; set; }
        [JsonProperty("color_text_secondary")]
        public string ColorTextSecondary { get; set; }
        [JsonProperty("radius_border")]
        public string RadiusBorder { get; set; }
    }
}