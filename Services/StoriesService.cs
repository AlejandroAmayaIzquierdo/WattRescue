using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class StoriesService(IHttpClientFactory httpClientFactory, WattDbContext dbContext)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private readonly WattDbContext _dbContext = dbContext;

    public async Task<Story?> GetStoryById(string id)
    {
        try
        {
            Story? existingStory = await _dbContext
                .Stories.Include(s => s.Parts)
                    .ThenInclude(p => p.Paragraphs)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStory is not null)
                return existingStory;

            Story? storyFromApi = await FetchStoryFromApi(id);

            if (storyFromApi is null)
                return null;

            _dbContext.Stories.Add(storyFromApi!);

            return storyFromApi;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching story by ID: {ex.Message}");
            return null;
        }
    }

    public async Task<Story?> FetchStoryFromApi(string id)
    {
        try
        {
            var _httpClient = _httpClientFactory.CreateClient("Wattpad");
            Story? story = await _httpClient.GetFromJsonAsync<Story>($"stories/{id}");

            return story;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching story from API: {ex.Message}");
            return null;
        }
    }

    public async Task SaveStoryAsync(Story story)
    {
        _dbContext.Stories.Update(story);
        await _dbContext.SaveChangesAsync();
    }
}
