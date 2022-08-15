using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOption
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// The shipment_id provided by the TMS.
        /// </summary>
        [JsonPropertyName("tms_reference")]
        public string TmsReference { get; set; }
        
        /// <summary>
        /// The carrier for the selected shipping option.
        /// </summary>
        [JsonPropertyName("carrier")]
        public string Carrier { get; set; }
        
        /// <summary>
        /// The type of the selected shipping option.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The method of the selected shipping option.
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }
        /// <summary>
        /// Price including tax.
        /// </summary>
        [JsonPropertyName("price")]
        public int Price { get; set; }
        /// <summary>
        /// Tax amount.
        /// </summary>
        [JsonPropertyName("tax_amount")]
        public int TaxAmount { get; set; }
        /// <summary>
        /// The tax rate of the selected shipping option.
        /// </summary>
        [JsonPropertyName("tax_rate")]
        public int TaxRate { get; set; }
        
        /// <summary>
        /// The location of the selected shipping option.
        /// </summary>
        [JsonPropertyName("location")]
        public OrderManagementShippingOptionLocation Location { get; set; }
        
        /// <summary>
        /// The chosen timeslot of the selected shipping option.
        /// </summary>
        [JsonPropertyName("timeslot")]
        public OrderManagementShippingOptionTimeslot Timeslot { get; set; }
        
        /// <summary>
        /// The chosen carrier product of the selected shipping option.
        /// </summary>
        [JsonPropertyName("carrier_product")]
        public OrderManagementShippingOptionCarrierProduct CarrierProduct { get; set; }
        
        /// <summary>
        /// Array consisting of add-ons selected by the consumer, may be empty.
        /// </summary>
        [JsonPropertyName("selected_addons")]
        public OrderManagementShippingOptionAddons SelectedAddons { get; set; }
        
        /// <summary>
        /// The class of the selected shipping option.
        /// </summary>
        [JsonPropertyName("class")]
        public string Class { get; set; }
    }
}