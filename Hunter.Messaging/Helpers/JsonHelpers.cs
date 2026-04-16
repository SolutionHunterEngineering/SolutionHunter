using System.Text.Json;

namespace Messaging.Helpers
{
    /// <summary>
    /// Small helper conversions between CLR objects and JsonElement for transport.
    /// Use these at the boundary where typed objects become transport JSON (and back).
    /// </summary>
    public static class JsonHelpers
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        /// <summary>
        /// Convert any object to a JsonElement suitable for storing inside TransportDTO.
        /// Returns a clone so it is safe to hold beyond the JsonDocument lifetime.
        /// If value is null, returns a JsonElement whose ValueKind == Null.
        /// </summary>
        public static JsonElement ToJsonElement(object? value, JsonSerializerOptions? options = null)
        {
            var opts = options ?? DefaultOptions;

            if (value is null)
            {
                // Parse a literal null into a JsonElement
                using var doc = JsonDocument.Parse("null");
                return doc.RootElement.Clone();
            }

            // Serialize to bytes and re-parse into JsonDocument so we get a JsonElement root.
            // Clone() so the returned element does not depend on the JsonDocument lifetime.
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), opts);
            using var parsed = JsonDocument.Parse(bytes);
            return parsed.RootElement.Clone();
        }

        /// <summary>
        /// Deserialize a JsonElement back into a strongly-typed object.
        /// If element is null (ValueKind.Null) returns default(T).
        /// </summary>
        public static T FromJsonElement<T>(JsonElement element, JsonSerializerOptions? options = null)
        {
            var opts = options ?? DefaultOptions;

            if (element.ValueKind == JsonValueKind.Null)
                return default!;

            // JsonElement.Deserialize<T> uses the supplied options
            return element.Deserialize<T>(opts)!;
        }
    }
}
