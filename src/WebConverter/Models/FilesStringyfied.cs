using System.Text.Json.Serialization;

namespace PDFtoImage.WebConverter.Models
{
	public record FilesStringyfied(
		[property: JsonPropertyName("title")]
		string? Title,

		[property: JsonPropertyName("text")]
		string? Text,

		[property: JsonPropertyName("url")]
		string? Url,

		[property: JsonPropertyName("files")]
		FileStringyfied[]? Files
	);
}