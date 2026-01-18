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
        return;
        Console.WriteLine("ScrapStoryJob executed at " + DateTime.Now);

        string STORY_ID = "199245730";

        Story? story = await _storiesService.GetStoryById(STORY_ID);

        if (story is null || string.IsNullOrEmpty(story.Url))
        {
            Console.WriteLine("Story not found.");
            return;
        }

        Console.WriteLine($"Title: {story?.Title}");

        foreach (var part in story!.Parts)
        {
            if (part.LastScrapedDate is null || part.LastScrapedDate < part.ModifyDate)
            {
                Console.WriteLine($"Scraping Part {part.Title}");
                string content = await _scrapperService.ScrapeChapterContentAsync(part);
                Paragraphs[] scrapedPart = _scrapperService.ParseChapterContent(content, part.Id);

                part.LastScrapedDate = DateTime.Now;

                Console.WriteLine(
                    $"Finished Scraping Part  {part.Title} at {part.LastScrapedDate}"
                );

                part.RawContent = content;

                part.Paragraphs.Clear();

                part.Paragraphs.AddRange(scrapedPart);

                await _storiesService.UpdateStoryAsync(story);
            }
            else
            {
                Console.WriteLine($"Skipping Part {part.Title}, already up to date.");
            }
        }

        story.LastScrapedDate = DateTime.Now;

        await _storiesService.UpdateStoryAsync(story);
    }
}
