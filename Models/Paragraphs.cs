namespace Models;

public class Paragraphs
{
    public required Guid Id { get; set; }
    public required int PartId { get; set; }
    public int ParagraphNumber { get; set; }
    public int Length { get; set; }

    public string? Content { get; set; }
}
