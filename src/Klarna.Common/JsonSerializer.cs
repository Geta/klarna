using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klarna.Common
{
    /// <summary>
    /// Default implementation of the IJsonSerializer interface.
    /// Inherit or nest and instance of this class and override its methods
    /// to allow inspection of payload.
    /// </summary>
    public class JsonSerializer : IJsonSerializer
    {
        /// <summary>
        /// Default json serialization options for the Klarna API
        /// </summary>
        protected JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            Converters =
            {
                new JsonStringEnumConverter(),
            }
        };

        /// <inheritdoc/>
        public string Serialize(object item)
        {
            return System.Text.Json.JsonSerializer.Serialize(item, SerializerOptions);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
    }
}
