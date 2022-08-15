using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Klarna.Common.Models
{
    public class Gui
    {
        /// <summary>
        /// An array of options to define the checkout behaviour. Supported options: disable_autofocux, minimal_confirmation.
        /// </summary>
        /// <remarks>Read only</remarks>
        [JsonPropertyName("options")]
        public ICollection<string> Options { get; set; }
    }
}
