using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace Pages;

public record ScrapeRequest(string Url);

public class ScrapeModel : PageModel
{
    public void OnGet(string? url)
    {
        ViewData["Url"] = url;
    }
}
