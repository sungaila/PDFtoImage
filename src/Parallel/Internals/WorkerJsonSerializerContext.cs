using SkiaSharp;
using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PDFtoImage.Parallel.Internals
{
    [JsonSourceGenerationOptions(
        GenerationMode = JsonSourceGenerationMode.Metadata,
        Converters = new[] { typeof(SKColorJsonConverter), typeof(RectangleFJsonConverter) })]
    [JsonSerializable(typeof(RenderOptions))]
    internal partial class WorkerJsonSerializerContext : JsonSerializerContext
    {
    }

    internal sealed class SKColorJsonConverter : JsonConverter<SKColor>
    {
        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetUInt32());

        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options) =>
            writer.WriteNumberValue((uint)value);
    }

    internal sealed class RectangleFJsonConverter : JsonConverter<RectangleF>
    {
        public override RectangleF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray || !reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException();
            var x = reader.GetSingle();

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException();
            var y = reader.GetSingle();

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException();
            var width = reader.GetSingle();

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                throw new JsonException();
            var height = reader.GetSingle();

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException();

            return new RectangleF(x, y, width, height);
        }

        public override void Write(Utf8JsonWriter writer, RectangleF value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Width);
            writer.WriteNumberValue(value.Height);
            writer.WriteEndArray();
        }
    }
}