using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class CheckoutMerchantUrls
    {
        /// <summary>
        /// URL of merchant terms and conditions. Should be different than checkout, confirmation and push URLs.(max 2000 characters)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("terms")]
        public string Terms { get; set; }
        /// <summary>
        /// URL of merchant cancellation terms.(max 2000 characters)
        /// </summary>
        [JsonPropertyName("cancellation_terms")]
        public string CancellationTerms { get; set; }
        /// <summary>
        /// URL of merchant checkout page. Should be different than terms, confirmation and push URLs. (max 2000 characters)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("checkout")]
        public string Checkout { get; set; }
        /// <summary>
        /// URL of merchant confirmation page. Should be different than checkout and confirmation URLs. (max 2000 characters)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("confirmation")]
        public string Confirmation { get; set; }
        /// <summary>
        /// URL that will be requested when an order is completed. Should be different than checkout and confirmation URLs. (max 2000 characters)
        /// </summary>
        /// <remarks>Required</remarks>
        [JsonPropertyName("push")]
        public string Push { get; set; }
        /// <summary>
        /// URL that will be requested for final merchant validation. (must be https, max 2000 characters)
        /// </summary>
        [JsonPropertyName("validation")]
        public string Validation { get; set; }
        /// <summary>
        /// URL for shipping option update. (must be https, max 2000 characters)
        /// </summary>
        [JsonPropertyName("shipping_option_update")]
        public string ShippingOptionUpdate { get; set; }
        /// <summary>
        /// URL for shipping, tax and purchase currency updates. Will be called on address changes. (must be https, max 2000 characters)
        /// </summary>
        [JsonPropertyName("address_update")]
        public string AddressUpdate { get; set; }
        /// <summary>
        /// URL for notifications on pending orders. (max 2000 characters)
        /// </summary>
        [JsonPropertyName("notification")]
        public string Notification { get; set; }
        /// <summary>
        /// URL for shipping, tax and purchase currency updates. Will be called on purchase country changes. (must be https, max 2000 characters)
        /// </summary>
        [JsonPropertyName("country_change")]
        public string CountryChange { get; set; }
    }
}
