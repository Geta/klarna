using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Checkout.Models
{
    public class MerchantRequested
    {
        /// <summary>
        /// Informs whether the AdditionalCheckbox is checked or not, when applicable.
        /// </summary>
        /// <remarks>Read only</remarks>
        [JsonPropertyName("additional_checkbox")]
        public bool AdditionalCheckbox { get; set; }
        /// <summary>
        /// Informs whether the AdditionalCheckboxes is checked or not, when applicable.
        /// </summary>
        /// <remarks>Read only</remarks>
        [JsonPropertyName("additional_checkboxes")]
        public ICollection<MerchantRequestedAdditionalCheckbox> AdditionalCheckboxes { get; set; }
    }
}
