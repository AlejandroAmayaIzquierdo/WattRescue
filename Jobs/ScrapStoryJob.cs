using HtmlAgilityPack;
using Models;
using PuppeteerSharp;
using Quartz;
using Services;

namespace Jobs;

public class ScrapStoryJob(StoriesService storiesService, ScrapperService scrapperService) : IJob
{
    public readonly StoriesService _storiesService = storiesService;
    public readonly ScrapperService _scrapperService = scrapperService;

    public async Task Execute(IJobExecutionContext context)
    {
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
                Console.WriteLine($"Scraping Part {part.PartNumber}: {part.Title}");
                string content = await _scrapperService.ScrapeChapterContentAsync(part);
                Paragraphs[] scrapedPart = _scrapperService.ParseChapterContent(content, part.Id);

                Console.WriteLine(
                    $"Scraped Part {part.PartNumber} Content Length: {scrapedPart.Length} characters"
                );

                // Here you can add code to save the scrapedPart back to the database if needed

                part.LastScrapedDate = DateTime.Now;

                Console.WriteLine(
                    $"Finished Scraping Part {part.PartNumber}: {part.Title} at {part.LastScrapedDate}"
                );

                part.RawContent = content;

                part.Paragraphs = [.. scrapedPart];
            }
            else
            {
                Console.WriteLine(
                    $"Skipping Part {part.PartNumber}: {part.Title}, already up to date."
                );
            }
        }

        await _storiesService.SaveStoryAsync(story);
    }
}
