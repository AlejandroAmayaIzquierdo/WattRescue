using System.Text.Json.Serialization;

namespace Models;

public class Paragraphs
{
    public int Id { get; set; }
    public int ParagraphNumber { get; set; }
    public int Length { get; set; }
    public string? Content { get; set; }

    // 🔑 FOREIGN KEY
    public int PartId { get; set; }

    // 🔁 NAVIGATION
    [JsonIgnore]
    public Part Part { get; set; } = null!;
}
