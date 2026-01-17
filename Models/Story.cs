namespace Models;

public class Story
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? ModifyDate { get; set; }

    public string? Cover { get; set; }
    public string? Description { get; set; }

    public string? Url { get; set; }

    public int NumParts { get; set; }

    public Parts[] Parts { get; set; } = [];
}
