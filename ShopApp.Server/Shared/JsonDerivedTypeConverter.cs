using System.Text.Json;
using System.Text.Json.Serialization;
using ShopApp.Models;

namespace ShopApp.Server.Shared;

public class JsonDerivedTypeConverter<T> : JsonConverter<T> where T : class
{
    private readonly Dictionary<string, Type> _typeMapping;

    public JsonDerivedTypeConverter()
    {
        _typeMapping = new Dictionary<string, Type>
        {
            { "tool", typeof(Tool) },
            { "accessory", typeof(Accessory) },
            { "clothing", typeof(Clothing) },
            { "masterclass", typeof(MasterClass) },
            { "yarn", typeof(Yarn) },
            { "yarnbobbin", typeof(YarnBobbin) }
        };
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDoc.RootElement;

        if (!rootElement.TryGetProperty("$type", out var typeProperty))
        {
            throw new JsonException("Missing $type property for polymorphic deserialization.");
        }

        var typeDiscriminator = typeProperty.GetString();
        if (string.IsNullOrEmpty(typeDiscriminator) || !_typeMapping.TryGetValue(typeDiscriminator, out var targetType))
        {
            throw new JsonException($"Unknown type discriminator: {typeDiscriminator}");
        }

        var jsonString = rootElement.GetRawText();
        return (T)JsonSerializer.Deserialize(jsonString, targetType, options)!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}