namespace Models;

public class Part
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? ModifyDate { get; set; }

    public DateTime? LastScrapedDate { get; set; } = null;

    public string? Url { get; set; }

    public int PartNumber { get; set; }
    public int Length { get; set; }

    public List<Paragraphs> Paragraphs { get; set; } = [];

    public string? RawContent { get; set; }
}
