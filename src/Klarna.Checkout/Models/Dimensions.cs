using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    /// <summary>
    /// Represents a product's dimensions
    /// </summary>
    public class Dimensions
    {
        /// <summary>
        /// The product's height as used in the merchant's webshop. Non-negative. Measured in millimeters.
        /// </summary>
        [JsonPropertyName("height")]
        public long Height { get; set; }

        /// <summary>
        /// The product's width as used in the merchant's webshop. Non-negative. Measured in millimeters.
        /// </summary>
        [JsonPropertyName("width")]
        public long Width { get; set; }

        /// <summary>
        /// The product's length as used in the merchant's webshop. Non-negative. Measured in millimeters.
        /// </summary>
        [JsonPropertyName("length")]
        public long Length { get; set; }
    }
}
