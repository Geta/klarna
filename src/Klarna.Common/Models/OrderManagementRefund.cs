using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class OrderManagementRefund
    {
        /// <summary>
        /// Refund ID
        /// </summary>
        [JsonPropertyName("refund_id")]
        public string RefundId { get; set; }

        /// <summary>
        /// Refunded amount in minor units.
        /// </summary>
        [JsonPropertyName("refunded_amount")]
        public int RefundedAmount { get; set; }

        /// <summary>
        /// Date for refund
        /// </summary>
        [JsonPropertyName("refunded_at")]
        public string RefundedAt { get; set; }

        /// <summary>
        /// Description of the refund shown to the customer. Max length is 255 characters.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Order lines for the refund shown to the customer. Optional but increases the customer experience. Maximum 1000 order lines.
        /// </summary>
        [JsonPropertyName("order_lines")]
        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
