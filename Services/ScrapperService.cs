using HtmlAgilityPack;
using Models;
using PuppeteerSharp;

namespace Services;

public class ScrapperService : IAsyncDisposable
{
    private IBrowser? _browser;

    private async Task InitializeBrowserAsync()
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
    }

    // public async Task<List<Part>> ScrapeStoryPartsAsync(Story story)
    // {
    //     var parts = new List<Part>();

    //     foreach (var part in story.Parts)
    //     {
    //         string content = await ScrapeChapterContentAsync(part);
    //         Paragraphs[] scrapedPart = ParseChapterContent(content, part.Id);
    //         parts.Add(scrapedPart);
    //     }

    //     return parts;
    // }

    public async Task<string> ScrapeChapterContentAsync(Part part)
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
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await CloseBrowserAsync();
    }
}
