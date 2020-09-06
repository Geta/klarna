using Newtonsoft.Json;

namespace Klarna.Common.Models
{
    public class OrderManagementShippingOption
    {
        /// <summary>
        /// Id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        /// <summary>
        /// Name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// The shipment_id provided by the TMS.
        /// </summary>
        [JsonProperty(PropertyName = "tms_reference")]
        public string TmsReference { get; set; }
        
        /// <summary>
        /// The carrier for the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "carrier")]
        public string Carrier { get; set; }
        
        /// <summary>
        /// The type of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        
        /// <summary>
        /// The method of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
        /// <summary>
        /// Price including tax.
        /// </summary>
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }
        /// <summary>
        /// Tax amount.
        /// </summary>
        [JsonProperty(PropertyName = "tax_amount")]
        public int TaxAmount { get; set; }
        /// <summary>
        /// The tax rate of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "tax_rate")]
        public int TaxRate { get; set; }
        
        /// <summary>
        /// The location of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "location")]
        public OrderManagementShippingOptionLocation Location { get; set; }
        
        /// <summary>
        /// The chosen timeslot of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "timeslot")]
        public OrderManagementShippingOptionTimeslot Timeslot { get; set; }
        
        /// <summary>
        /// The chosen carrier product of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "carrier_product")]
        public OrderManagementShippingOptionCarrierProduct CarrierProduct { get; set; }
        
        /// <summary>
        /// Array consisting of add-ons selected by the consumer, may be empty.
        /// </summary>
        [JsonProperty(PropertyName = "selected_addons")]
        public OrderManagementShippingOptionAddons SelectedAddons { get; set; }
        
        /// <summary>
        /// The class of the selected shipping option.
        /// </summary>
        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; }
    }
}