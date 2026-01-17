namespace Models;

public class Parts
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? ModifyDate { get; set; }

    public string? Url { get; set; }

    public int PartNumber { get; set; }
    public int Length { get; set; }

    public Paragraphs[] Paragraphs { get; set; } = [];

    public string? RawContent { get; set; }
}
