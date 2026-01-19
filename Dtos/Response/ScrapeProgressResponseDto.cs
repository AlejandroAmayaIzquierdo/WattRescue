namespace DTOs;

public class ScrapeProgressResponseDto
{
    public string? StoryId { get; set; }
    public string? StoryTitle { get; set; }
    public string? PartId { get; set; }
    public string? Message { get; set; }
    public int Percentage { get; set; }

    public bool IsCompleted => Percentage >= 100;

    public bool IsError { get; set; }

    public bool HasStarted => Percentage > 0;
}
