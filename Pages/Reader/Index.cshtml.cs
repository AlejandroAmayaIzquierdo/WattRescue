using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

namespace Pages;

public class ReaderModel(StoriesService storiesService) : PageModel
{
    public readonly StoriesService _storiesService = storiesService;
    public string? StoryId { get; set; }
    public int PageNumber { get; set; } = 1;

    public int TotalPages => Story?.NumParts ?? 0;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public string? PreviousPageUrl =>
        HasPreviousPage ? $"/reader/{StoryId}?page={PageNumber - 1}" : null;

    public string? NextPageUrl => HasNextPage ? $"/reader/{StoryId}?page={PageNumber + 1}" : null;

    public string? CurrentChapterTitle =>
        Story is not null && PageNumber > 0 && PageNumber <= TotalPages
            ? Story?.Parts.ElementAtOrDefault(PageNumber - 1)?.Title ?? null
            : null;
    public Story? Story { get; internal set; }

    public List<string> Paragraphs { get; internal set; } = [];

    public async Task<IActionResult> OnGet([FromQuery] int? page)
    {
        StoryId = Request.RouteValues["id"]?.ToString();

        if (StoryId == null)
            return RedirectToPage("/Index");

        PageNumber = page ?? 1;

        Story = await _storiesService.GetStoryById(StoryId);

        if (Story == null)
            return RedirectToPage("/Index");

        if (PageNumber < 1 || PageNumber > Story.NumParts)
            return RedirectToPage($"/Index");

        Paragraphs =
            Story
                .Parts.OrderBy(part => part.Id)
                .ElementAtOrDefault(PageNumber - 1)
                ?.Content?.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries)
                .ToList()
            ?? [];

        return Page();
    }

    public async Task<(int Id, string Title)> GetChapterInfo(int pageNumber = -1)
    {
        if (StoryId == null)
            return (0, string.Empty);

        if (pageNumber == -1)
            pageNumber = PageNumber;
        var chapters = await _storiesService.GetChaptersList(StoryId);
        return chapters.ElementAtOrDefault(pageNumber - 1);
    }
}
