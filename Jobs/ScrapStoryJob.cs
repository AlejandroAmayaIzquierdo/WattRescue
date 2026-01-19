using Models;
using Quartz;
using Services;

namespace Jobs;

public class ScrapStoryJob(StoriesService storiesService, ScrapperService scrapperService) : IJob
{
    public readonly StoriesService _storiesService = storiesService;
    public readonly ScrapperService _scrapperService = scrapperService;

    public async Task Execute(IJobExecutionContext context)
    {
        var stories = await _storiesService.GetAllStories();

        foreach (var story in stories)
        {
            if (story.LastScrapedDate is null || story.LastScrapedDate < story.ModifyDate)
            {
                List<Part>? parts = await _scrapperService.ScrapeStoryPartsAsync(story);
                story.LastScrapedDate = DateTime.UtcNow;

                story.Parts.Clear();

                if (parts is not null)
                    story.Parts.AddRange(parts);

                await _storiesService.UpdateStoryAsync(story);

                await _scrapperService.CloseBrowserAsync();
            }
        }
    }
}
