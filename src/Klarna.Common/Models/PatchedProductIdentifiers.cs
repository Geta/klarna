using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class PatchedProductIdentifiers
    {
        [JsonProperty("category_path")]
        public string CategoryPath { get; set; }

        [JsonProperty("global_trade_item_number")]
        public string GlobalTradeItemNumber { get; set; }

        [JsonProperty("manufacturer_part_number")]
        public string ManuFacturerPartNumber { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }
    }
}
