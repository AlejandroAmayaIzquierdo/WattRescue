using System.Collections.Concurrent;
using DTOs;
using HtmlAgilityPack;
using Models;
using PuppeteerSharp;

namespace Services;

public class ScrapperService
{
    private IBrowser? _browser;

    private static readonly ConcurrentDictionary<
        string,
        ScrapeProgressResponseDto
    > ProgressByStoryId = new();

    public ScrapeProgressResponseDto? Progress(string storyId)
    {
        if (ProgressByStoryId.TryGetValue(storyId, out var progress))
            return progress;

        return null;
    }

    private async Task InitializeBrowserAsync()
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
    }

    public async Task<List<Part>?> ScrapeStoryPartsAsync(Story story)
    {
        try
        {
            var parts = new List<Part>();

            ProgressByStoryId[story.Id] = new ScrapeProgressResponseDto()
            {
                StoryId = story.Id,
                StoryTitle = story.Title,
                Message = "Scraping started",
                Percentage = 0,
                IsError = false,
            };

            int partIndex = 1;

            foreach (var part in story.Parts)
            {
                UpdateProgress(
                    story.Id,
                    $"Scraped part {part.Title}",
                    Math.Min(100, (int)((partIndex - 1) / (double)story.Parts.Count * 100)),
                    part.Id.ToString()
                );

                partIndex++;

                if (part.LastScrapedDate is not null && part.LastScrapedDate >= part.ModifyDate)
                {
                    parts.Add(part);
                    continue;
                }

                string? content = await ScrapeChapterContentAsync(part);

                if (content is null)
                    continue;

                Paragraphs[] scrapedPart = ParseChapterContent(content, part.Id);
                part.RawContent = content;

                part.Content = string.Join("\n\n", scrapedPart.Select(p => p.Content));

                part.LastScrapedDate = DateTime.UtcNow;

                parts.Add(part);
            }

            return parts;
        }
        catch (Exception ex)
        {
            UpdateProgress(
                story.Id,
                $"Error during scraping: {ex.Message}",
                100,
                string.Empty,
                isError: true
            );
            return null;
        }
        finally
        {
            UpdateProgress(story.Id, "Scraping completed", 100, string.Empty);
        }
    }

    public async Task<string?> ScrapeChapterContentAsync(Part part)
    {
        try
        {
            if (_browser == null)
                await InitializeBrowserAsync();

            if (_browser == null)
                throw new InvalidOperationException("Browser initialization failed.");

            using var page = await _browser.NewPageAsync();
            await page.GoToAsync(part.Url);
            await Task.Delay(2000); // Wait for 2 seconds to ensure the page loads completely

            if (await page.QuerySelectorAsync("#onetrust-reject-all-handler") is not null)
                await page.ClickAsync("#onetrust-reject-all-handler"); // Click the "Reject All" button on the cookie consent banner

            await page.EvaluateExpressionAsync(
                $"document.getElementById('story-part-comments').scrollIntoView();"
            ); // Scroll to the comments section to load all content

            await Task.Delay(1000); // Wait for 1 seconds to ensure the page loads completely

            string content = await page.GetContentAsync();
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping chapter content: {ex.Message}");
            return null;
        }
    }

    public Paragraphs[] ParseChapterContent(string content, int partId)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(content);

        return
        [
            .. htmlDoc
                .DocumentNode.Descendants()
                .Where(n => n.Name == "p" && n.GetAttributeValue("data-p-id", "") != "")
                .Select(n => new Paragraphs { PartId = partId, Content = n.InnerText.Trim() }),
        ];
    }

    public async Task CloseBrowserAsync()
    {
        try
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
        }
        catch
        {
            Console.WriteLine("Error closing browser.");
        }
    }

    private static void UpdateProgress(
        string storyId,
        string message,
        int percentage,
        string partId,
        bool isError = false
    )
    {
        if (ProgressByStoryId.TryGetValue(storyId, out var progress))
        {
            Console.WriteLine(
                $"Progress Update - StoryId: {storyId}, PartId: {partId}, Percentage: {percentage}, Message: {message}, IsError: {isError}"
            );
            progress.PartId = partId;
            progress.IsError = isError;
            progress.Message = message;
            progress.Percentage = percentage;
        }
    }
}
