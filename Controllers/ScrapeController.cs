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
public class ScrapeController(
    ScrapperService scrapperService,
    StoriesService storiesService,
    IServiceScopeFactory serviceScopeFactory
) : ControllerBase
{
    private readonly ScrapperService _scrapperService = scrapperService;

    private readonly StoriesService _storiesService = storiesService;

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

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
            try
            {
                List<Part>? scrapedParts = await _scrapperService.ScrapeStoryPartsAsync(story);

                if (scrapedParts is null)
                    return;

                using var scope = _serviceScopeFactory.CreateScope();
                var scopedStoriesService =
                    scope.ServiceProvider.GetRequiredService<StoriesService>();

                // Guardar cada parte individualmente para evitar timeout
                foreach (var part in scrapedParts)
                {
                    await scopedStoriesService.UpdatePartAsync(part);
                }

                // Actualizar la fecha de scraping del story
                await scopedStoriesService.UpdateStoryLastScrapedAsync(storyId);

                await _scrapperService.CloseBrowserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in background scrape task: {ex.Message}");
            }
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
