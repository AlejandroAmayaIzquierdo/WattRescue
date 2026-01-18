using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

namespace Pages;

public record ScrapeRequest(string Url);

public class ScrapeModel(StoriesService storiesService) : PageModel
{
    public Story? Story { get; set; }

    private readonly StoriesService _storiesService = storiesService;

    [BindProperty]
    public ScrapeRequest Request { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPost()
    {
        Console.WriteLine($"Scrape requested for URL: {Request.Url}");

        string storyId = ExtractStoryId(Request.Url);

        await _storiesService.GetStoryById(storyId);
        return RedirectToPage("/Index");
    }

    private static string ExtractStoryId(string url)
    {
        try
        {
            Uri uri = new(url);
            string[] segments = uri.Segments;
            if (segments.Length >= 3 && segments[1].TrimEnd('/') == "story")
            {
                string storynameUrl = segments[2];
                string storyId = storynameUrl.Split('-')[0];
                return storyId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting story ID: {ex.Message}");
        }
        return string.Empty;
    }
}
