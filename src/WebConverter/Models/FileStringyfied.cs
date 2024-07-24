using System;
using System.Text.Json.Serialization;

namespace PDFtoImage.WebConverter.Models
{
    public partial record FileStringyfied(
        [property: JsonPropertyName("name")]
        [property: JsonRequired]
        string Name,

        [property: JsonPropertyName("lastModified")]
        [property: JsonRequired]
        [property: JsonConverter(typeof(UnixDateTimeConverter))]
        DateTimeOffset LastModified,

        [property: JsonPropertyName("size")]
        [property: JsonRequired]
        int Size,

        [property: JsonPropertyName("type")]
        [property: JsonRequired]
        string Type,

        [property: JsonPropertyName("data")]
        [property: JsonRequired]
        string Data
    )
    {
        public string GetData() => Data[(Data.IndexOf("base64,") + "base64,".Length)..];
    };
}