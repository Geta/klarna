using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    /// <summary>
    /// Represents shipping attributes of an order item
    /// </summary>
    public class ShippingAttributes
    {
        /// <summary>
        /// The product's weight as used in the merchant's webshop. Non-negative. Measured in grams.
        /// </summary>
        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        /// <summary>
        /// The product's dimensions: height, width and length. Each dimension is of type Long.
        /// </summary>
        [JsonPropertyName("dimensions")]
        public Dimensions Dimensions { get; set; }

        /// <summary>
        /// The product's extra features
        /// </summary>
        [JsonPropertyName("tags")]
        public ICollection<string> Tags { get; set; }
    }
}
