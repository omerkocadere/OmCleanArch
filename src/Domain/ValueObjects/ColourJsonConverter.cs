using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanArch.Domain.ValueObjects;

public class ColourJsonConverter : JsonConverter<Colour>
{
    public override Colour Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Colour value is null or empty.");
        return Colour.From(value);
    }

    public override void Write(Utf8JsonWriter writer, Colour value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
