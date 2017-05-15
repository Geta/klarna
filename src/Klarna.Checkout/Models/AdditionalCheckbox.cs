using System.Collections.Generic;
using Newtonsoft.Json;

namespace Klarna.Checkout.Models
{
    public class AdditionalCheckbox : Rest.Models.Model
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("checked")]
        public bool? Checked { get; set; }

        [JsonProperty("required")]
        public bool? Required { get; set; }
    }
}
