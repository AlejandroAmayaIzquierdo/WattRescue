using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

namespace Pages;

public class IndexModel(StoriesService storiesService) : PageModel
{
    public readonly StoriesService _storiesService = storiesService;

    public IList<Story> Stories { get; set; } = [];

    public async Task OnGet()
    {
        Stories = await _storiesService.GetAllStories();
    }
}
