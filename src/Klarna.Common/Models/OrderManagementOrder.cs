using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Klarna.Common.Models
{
    public class OrderManagementOrder
    {
        /// <summary>
        /// The unique order ID. Cannot be longer than 255 characters.
        /// </summary>
        /// <value>The unique order ID. Cannot be longer than 255 characters.</value>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// The order status.
        /// </summary>
        /// <value>The order status.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonPropertyName("status")]
        public OrderManagementOrderStatus Status { get; set; }

        /// <summary>
        /// Fraud status for the order. Either ACCEPTED, PENDING or REJECTED.
        /// </summary>
        /// <value>Fraud status for the order. Either ACCEPTED, PENDING or REJECTED.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonPropertyName("fraud_status")]
        public OrderManagementFraudStatus FraudStatus { get; set; }

        /// <summary>
        /// The order amount in minor units. That is the smallest currency unit available such as cent or penny.
        /// </summary>
        /// <value>The order amount in minor units. That is the smallest currency unit available such as cent or penny.</value>
        [JsonPropertyName("order_amount")]
        public int OrderAmount { get; set; }

        /// <summary>
        /// The original order amount. In minor units.
        /// </summary>
        /// <value>The original order amount. In minor units.</value>
        [JsonPropertyName("original_order_amount")]
        public int OriginalOrderAmount { get; set; }

        /// <summary>
        /// The total amount of all captures. In minor units.
        /// </summary>
        /// <value>The total amount of all captures. In minor units.</value>
        [JsonPropertyName("captured_amount")]
        public int CapturedAmount { get; set; }

        /// <summary>
        /// The total amount of refunded for this order. In minor units.
        /// </summary>
        /// <value>The total amount of refunded for this order. In minor units.</value>
        [JsonPropertyName("refunded_amount")]
        public int RefundedAmount { get; set; }

        /// <summary>
        /// The remaining authorized amount for this order. To increase the `remaining_authorized_amount` the `order_amount` needs to be increased.
        /// </summary>
        /// <value>The remaining authorized amount for this order. To increase the `remaining_authorized_amount` the `order_amount` needs to be increased.</value>
        [JsonPropertyName("remaining_authorized_amount")]
        public int RemainingAuthorizedAmount { get; set; }

        /// <summary>
        /// The currency for this order. Specified in ISO 4217 format.
        /// </summary>
        /// <value>The currency for this order. Specified in ISO 4217 format.</value>
        [JsonPropertyName("purchase_currency")]
        public string PurchaseCurrency { get; set; }

        /// <summary>
        /// The customers locale. Specified according to RFC 1766.
        /// </summary>
        /// <value>The customers locale. Specified according to RFC 1766.</value>
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// An array of order_line objects. Each line represents one item in the cart.
        /// </summary>
        /// <value>An array of order_line objects. Each line represents one item in the cart.</value>
        [JsonPropertyName("order_lines")]
        public ICollection<OrderLine> OrderLines { get; set; }

        /// <summary>
        /// The order number that the merchant should assign to the order. This is how a customer would reference the purchase they made. If supplied, it is labeled as the Order Number within post purchase communications as well as the Klarna App.
        /// </summary>
        /// <value>The order number that the merchant should assign to the order. This is how a customer would reference the purchase they made. If supplied, it is labeled as the Order Number within post purchase communications as well as the Klarna App.</value>
        [JsonPropertyName("merchant_reference1")]
        public string MerchantReference1 { get; set; }

        /// <summary>
        /// Can be used to store your internal reference to the order. This is generally an internal reference number that merchants use as alternate identifier that matches their internal ERP or Order Management system.
        /// </summary>
        /// <value>Can be used to store your internal reference to the order. This is generally an internal reference number that merchants use as alternate identifier that matches their internal ERP or Order Management system.</value>
        [JsonPropertyName("merchant_reference2")]
        public string MerchantReference2 { get; set; }

        /// <summary>
        /// A Klarna generated reference that is shorter than the Klarna Order Id and is used as a customer friendly reference. It is most often used as a reference when Klarna is communicating with the customer with regard to payment statuses.
        /// </summary>
        /// <value>A Klarna generated reference that is shorter than the Klarna Order Id and is used as a customer friendly reference. It is most often used as a reference when Klarna is communicating with the customer with regard to payment statuses.</value>
        [JsonPropertyName("klarna_reference")]
        public string KlarnaReference { get; set; }

        /// <summary>
        /// Information about the customer placing the order.
        /// </summary>
        /// <value>Information about the customer placing the order.</value>
        [JsonPropertyName("customer")]
        public OrderManagementCustomer Customer { get; set; }

        /// <summary>
        /// Customer billing address.
        /// </summary>
        /// <value>Customer billing address.</value>
        [JsonPropertyName("billing_address")]
        public OrderManagementAddressInfo BillingAddress { get; set; }

        /// <summary>
        /// Customer shipping address.
        /// </summary>
        /// <value>Customer shipping address.</value>
        [JsonPropertyName("shipping_address")]
        public OrderManagementAddressInfo ShippingAddress { get; set; }

        /// <summary>
        /// The time for the purchase. Formatted according to ISO 8601.
        /// </summary>
        /// <value>The time for the purchase. Formatted according to ISO 8601.</value>
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// The purchase country. Formatted according to ISO 3166-1 alpha-2.
        /// </summary>
        /// <value>The purchase country. Formatted according to ISO 3166-1 alpha-2.</value>
        [JsonPropertyName("purchase_country")]
        public string PurchaseCountry { get; set; }

        /// <summary>
        /// Order expiration time. The order can only be captured until this time. Formatted according to ISO 8601.
        /// </summary>
        /// <value>Order expiration time. The order can only be captured until this time. Formatted according to ISO 8601.</value>
        [JsonPropertyName("expires_at")]
        public string ExpiresAt { get; set; }

        /// <summary>
        /// List of captures for this order.
        /// </summary>
        /// <value>List of captures for this order.</value>
        [JsonPropertyName("captures")]
        public ICollection<OrderManagementCapture> Captures { get; set; }

        /// <summary>
        /// List of refunds for this order.
        /// </summary>
        /// <value>List of refunds for this order.</value>
        [JsonPropertyName("refunds")]
        public ICollection<OrderManagementRefund> Refunds { get; set; }

        /// <summary>
        /// Text field for storing data about the order. Set at order creation.
        /// </summary>
        /// <value>Text field for storing data about the order. Set at order creation.</value>
        [JsonPropertyName("merchant_data")]
        public string MerchantData { get; set; }

        /// <summary>
        /// Initial payment method for this order
        /// </summary>
        /// <value>Initial payment method for this order</value>
        [JsonPropertyName("initial_payment_method")]
        public OrderManagementInitialPaymentMethod InitialPaymentMethod { get; set; }
        
        /// <summary>
        /// Current shipping options selected by the customer.
        /// </summary>
        /// <value>Current shipping options selected by the customer.</value>
        [JsonPropertyName("selected_shipping_option")]
        public OrderManagementShippingOption SelectedShippingOption { get; set; }
    }
}
