using HtmlAgilityPack;
using Models;
using PuppeteerSharp;
using Quartz;
using Services;

namespace Jobs;

public class ScrapStoryJob(StoriesService storiesService) : IJob
{
    public readonly StoriesService _storiesService = storiesService;

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

        var browserFetcher = new BrowserFetcher();

        await browserFetcher.DownloadAsync();

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false });

        var page = await browser.NewPageAsync();

        await page.GoToAsync(story!.Parts[0].Url);

        await Task.Delay(2000); // Wait for 2 seconds to ensure the page loads completely

        await page.ClickAsync("#onetrust-reject-all-handler"); // Click the "Reject All" button on the cookie consent banner

        await page.EvaluateExpressionAsync(
            $"document.getElementById('story-part-comments').scrollIntoView();"
        ); // Scroll to the comments section to load all content

        await Task.Delay(1000); // Wait for 1 seconds to ensure the page loads completely

        string content = await page.GetContentAsync();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(content);

        var paragraphs = htmlDoc
            .DocumentNode.Descendants()
            .Where(n => n.Name == "p" && n.GetAttributeValue("data-p-id", "") != "")
            .Select(n => n.InnerText.Trim())
            .ToList();

        foreach (var paragraph in paragraphs)
            Console.WriteLine(paragraph);
    }
}
