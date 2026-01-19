using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScrapeController(ScrapperService scrapperService, StoriesService storiesService)
    : ControllerBase
{
    private readonly ScrapperService _scrapperService = scrapperService;

    private readonly StoriesService _storiesService = storiesService;

    public record StartScrapeRequest(string StoryUrl);

    [HttpPost("start")]
    public async Task<IActionResult> StartScrape([FromBody] StartScrapeRequest request)
    {
        string storyId = _storiesService.ExtractStoryId(request.StoryUrl);
        Story? story = await _storiesService.GetStoryById(storyId);
        if (story is null)
            return NotFound(new { Message = "Story not found." });

        _ = Task.Run(async () =>
        {
            List<Part>? parts = await _scrapperService.ScrapeStoryPartsAsync(story);

            story.Parts.Clear();
            if (parts is not null)
                story.Parts.AddRange(parts);

            story.LastScrapedDate = DateTime.UtcNow;

            await _storiesService.UpdateStoryAsync(story);

            await _scrapperService.CloseBrowserAsync();
        });
        return Ok(story);
    }

    [HttpGet("progress/{storyId}")]
    public async Task<IActionResult> GetScrapeProgress(string storyId)
    {
        var progress = _scrapperService.Progress(storyId);

        if (progress == null || progress.StoryId != storyId)
            return NotFound(new { Message = "No ongoing scrape for the specified story ID." });

        return Ok(progress);
    }
}
