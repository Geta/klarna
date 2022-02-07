using System.Collections.Generic;
using System.Text.Json.Serialization;
using Klarna.Common.Models;

namespace Klarna.Checkout.Models
{
    public class CheckoutOrder
    {
        /// <summary>
        /// The unique order ID (max 255 characters).
        /// </summary>
        /// <value>The unique order ID (max 255 characters).</value>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// The merchant name (max 255 characters).
        /// </summary>
        /// <value>The merchant name (max 255 characters).</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// ISO 3166 alpha-2 purchase country.
        /// </summary>
        /// <value>ISO 3166 alpha-2 purchase country.</value>
        [JsonPropertyName("purchase_country")]
        public string PurchaseCountry { get; set; }

        /// <summary>
        /// ISO 4217 purchase currency.
        /// </summary>
        /// <value>ISO 4217 purchase currency.</value>
        [JsonPropertyName("purchase_currency")]
        public string PurchaseCurrency { get; set; }

        /// <summary>
        /// RFC 1766 customer's locale.
        /// </summary>
        /// <value>RFC 1766 customer's locale.</value>
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// The current status of the order.
        /// </summary>
        /// <value>The current status of the order.</value>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Once the customer has provided any data in the checkout iframe, updates to this object will be ignored (without generating an error).
        /// </summary>
        /// <value>Once the customer has provided any data in the checkout iframe, updates to this object will be ignored (without generating an error).</value>
        [JsonPropertyName("billing_address")]
        public CheckoutAddressInfo BillingCheckoutAddress { get; set; }

        /// <summary>
        /// Unless the customer has explicitly chosen to enter a separate shipping address, this is a clone of billing_address.
        /// </summary>
        /// <value>Unless the customer has explicitly chosen to enter a separate shipping address, this is a clone of billing_address.</value>
        [JsonPropertyName("shipping_address")]
        public CheckoutAddressInfo ShippingCheckoutAddress { get; set; }

        /// <summary>
        /// Non-negative, minor units. Total amount of the order, including tax and any discounts.
        /// </summary>
        /// <value>Non-negative, minor units. Total amount of the order, including tax and any discounts.</value>
        [JsonPropertyName("order_amount")]
        public int OrderAmount { get; set; }

        /// <summary>
        /// Non-negative, minor units. The total tax amount of the order.
        /// </summary>
        /// <value>Non-negative, minor units. The total tax amount of the order.</value>
        [JsonPropertyName("order_tax_amount")]
        public int OrderTaxAmount { get; set; }

        /// <summary>
        /// The applicable order lines (max 1000)
        /// </summary>
        /// <value>The applicable order lines (max 1000)</value>
        [JsonPropertyName("order_lines")]
        public ICollection<OrderLine> OrderLines { get; set; }

        /// <summary>
        /// Information about the liable customer of the order.
        /// </summary>
        /// <value>Information about the liable customer of the order.</value>
        [JsonPropertyName("customer")]
        public CheckoutCustomer CheckoutCustomer { get; set; }

        /// <summary>
        /// The merchant_urls object.
        /// </summary>
        /// <value>The merchant_urls object.</value>
        [JsonPropertyName("merchant_urls")]
        public CheckoutMerchantUrls MerchantUrls { get; set; }

        /// <summary>
        /// The HTML snippet that is used to render the checkout in an iframe.
        /// </summary>
        /// <value>The HTML snippet that is used to render the checkout in an iframe.</value>
        [JsonPropertyName("html_snippet")]
        public string HtmlSnippet { get; set; }

        /// <summary>
        /// Used for storing merchant's internal order number or other reference. If set, will be shown on the confirmation page as \"order number\" (max 255 characters).
        /// </summary>
        /// <value>Used for storing merchant's internal order number or other reference. If set, will be shown on the confirmation page as \"order number\" (max 255 characters).</value>
        [JsonPropertyName("merchant_reference1")]
        public string MerchantReference1 { get; set; }

        /// <summary>
        /// Used for storing merchant's internal order number or other reference (max 255 characters).
        /// </summary>
        /// <value>Used for storing merchant's internal order number or other reference (max 255 characters).</value>
        [JsonPropertyName("merchant_reference2")]
        public string MerchantReference2 { get; set; }

        /// <summary>
        /// ISO 8601 datetime. When the merchant created the order.
        /// </summary>
        /// <value>ISO 8601 datetime. When the merchant created the order.</value>
        [JsonPropertyName("started_at")]
        public string StartedAt { get; set; }

        /// <summary>
        /// ISO 8601 datetime. When the customer completed the order.
        /// </summary>
        /// <value>ISO 8601 datetime. When the customer completed the order.</value>
        [JsonPropertyName("completed_at")]
        public string CompletedAt { get; set; }

        /// <summary>
        /// ISO 8601 datetime. When the order was last modified.
        /// </summary>
        /// <value>ISO 8601 datetime. When the order was last modified.</value>
        [JsonPropertyName("last_modified_at")]
        public string LastModifiedAt { get; set; }

        /// <summary>
        /// Options for this purchase.
        /// </summary>
        /// <value>Options for this purchase.</value>
        [JsonPropertyName("options")]
        public CheckoutOptions CheckoutOptions { get; set; }

        /// <summary>
        /// Additional purchase information required for some industries.
        /// </summary>
        /// <value>Additional purchase information required for some industries.</value>
        [JsonPropertyName("attachment")]
        public Attachment Attachment { get; set; }

        /// <summary>
        /// List of external payment methods.
        /// </summary>
        /// <value>List of external payment methods.</value>
        [JsonPropertyName("external_payment_methods")]
        public ICollection<PaymentProvider> ExternalPaymentMethods { get; set; }

        /// <summary>
        /// List of external checkouts.
        /// </summary>
        /// <value>List of external checkouts.</value>
        [JsonPropertyName("external_checkouts")]
        public ICollection<PaymentProvider> ExternalCheckouts { get; set; }

        /// <summary>
        /// A list of countries (ISO 3166 alpha-2). Default is purchase_country only.
        /// </summary>
        /// <value>A list of countries (ISO 3166 alpha-2). Default is purchase_country only.</value>
        [JsonPropertyName("shipping_countries")]
        public ICollection<string> ShippingCountries { get; set; }

        /// <summary>
        /// A list of shipping options available for this order.
        /// </summary>
        /// <value>A list of shipping options available for this order.</value>
        [JsonPropertyName("shipping_options")]
        public ICollection<ShippingOption> ShippingOptions { get; set; }

        /// <summary>
        /// Pass through field (max 6000 characters).
        /// </summary>
        /// <value>Pass through field (max 6000 characters).</value>
        [JsonPropertyName("merchant_data")]
        public string MerchantData { get; set; }

        /// <summary>
        /// The gui object.
        /// </summary>
        /// <value>The gui object.</value>
        [JsonPropertyName("gui")]
        public Gui Gui { get; set; }

        /// <summary>
        /// Stores merchant requested data.
        /// </summary>
        /// <value>Stores merchant requested data.</value>
        [JsonPropertyName("merchant_requested")]
        public MerchantRequested MerchantRequested { get; set; }

        /// <summary>
        /// Current shipping options selected by the customer.
        /// </summary>
        /// <value>Current shipping options selected by the customer.</value>
        [JsonPropertyName("selected_shipping_option")]
        public ShippingOption SelectedShippingOption { get; set; }

        /// <summary>
        /// Indicates whether this purchase will create a token that can be used by the merchant to create recurring purchases. This must be enabled for the merchant to use. Default: false
        /// </summary>
        /// <value>Indicates whether this purchase will create a token that can be used by the merchant to create recurring purchases. This must be enabled for the merchant to use. Default: false</value>
        [JsonPropertyName("recurring")]
        public bool Recurring { get; set; }

        /// <summary>
        /// Token to be used when creating recurring orders.
        /// </summary>
        /// <value>Token to be used when creating recurring orders.</value>
        [JsonPropertyName("recurring_token")]
        public string RecurringToken { get; set; }

        /// <summary>
        /// Description recurring subscription.
        /// </summary>
        /// <value>Description recurring subscription.</value>
        [JsonPropertyName("recurring_description")]
        public string RecurringDescription { get; set; }

        /// <summary>
        /// A list of countries (ISO 3166 alpha-2) to specify allowed billing countries.
        /// </summary>
        /// <value>A list of countries (ISO 3166 alpha-2) to specify allowed billing countries.</value>
        [JsonPropertyName("billing_countries")]
        public ICollection<string> BillingCountries { get; set; }

        /// <summary>
        /// The product's extra features
        /// </summary>
        /// <value>The product's extra features</value>
        [JsonPropertyName("tags")]
        public ICollection<string> Tags { get; set; }
    }
}
