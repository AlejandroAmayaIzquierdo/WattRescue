using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class StoriesService(IHttpClientFactory httpClientFactory, WattDbContext dbContext)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private readonly WattDbContext _dbContext = dbContext;

    public IList<Story> GetAllStories()
    {
        return [.. _dbContext.Stories];
    }

    public async Task<Story?> GetStoryById(string id)
    {
        try
        {
            Story? existingStory = await _dbContext
                .Stories.Include(s => s.Parts)
                    .ThenInclude(p => p.Paragraphs)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStory is not null)
                return existingStory;

            Story? storyFromApi = await FetchStoryFromApi(id);

            if (storyFromApi is null)
                return null;

            _dbContext.Stories.Add(storyFromApi);

            await _dbContext.SaveChangesAsync();

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

    public async Task<Story> UpdateStoryAsync(Story story)
    {
        // If the entity is already tracked, just save changes
        // Otherwise, attach it and mark as modified
        var entry = _dbContext.Stories.Entry(story);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Stories.Update(story);
        }

        await _dbContext.SaveChangesAsync();

        return story;
    }

    public async Task UpdatePartAsync(Part part)
    {
        var entry = _dbContext.Entry(part);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Parts.Update(part);
        }

        await _dbContext.SaveChangesAsync();
    }
}
